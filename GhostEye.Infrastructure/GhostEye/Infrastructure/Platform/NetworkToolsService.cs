using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;

namespace GhostEye.Infrastructure.Platform;

public sealed class NetworkToolsService(ILocalizer loc) : INetworkToolsService
{
	private const string SpeedHost = "https://speed.cloudflare.com";

	private const long DownloadBytes = 52428800L;

	private const int UploadBytes = 15728640;

	private static readonly TimeSpan PhaseLimit = TimeSpan.FromSeconds(12.0);

	private static readonly HttpClient Http = new HttpClient
	{
		Timeout = TimeSpan.FromSeconds(40.0)
	};

	public IReadOnlyList<PingTarget> PingTargets { get; } = new _003C_003Ez__ReadOnlyArray<PingTarget>(new PingTarget[6]
	{
		new PingTarget("cloudflare", "1.1.1.1"),
		new PingTarget("google", "8.8.8.8"),
		new PingTarget("epicEu", "ping-eu.ds.on.epicgames.com"),
		new PingTarget("awsEu", "dynamodb.eu-central-1.amazonaws.com"),
		new PingTarget("steam", "store.steampowered.com"),
		new PingTarget("xbox", "xbox.com")
	});

	public async Task<PingResult> PingAsync(PingTarget target, int count = 8, CancellationToken ct = default(CancellationToken))
	{
		using Ping ping = new Ping();
		List<long> times = new List<long>();
		for (int i = 0; i < count; i++)
		{
			ct.ThrowIfCancellationRequested();
			try
			{
				PingReply pingReply = await ping.SendPingAsync(target.Host, 1500).ConfigureAwait(continueOnCapturedContext: false);
				if (pingReply.Status == IPStatus.Success)
				{
					times.Add(pingReply.RoundtripTime);
				}
			}
			catch (PingException)
			{
			}
			await Task.Delay(150, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (times.Count == 0)
		{
			return new PingResult(target, null, null, 100);
		}
		double value = ((times.Count > 1) ? times.Zip(times.Skip(1), (long a, long b) => Math.Abs(a - b)).Average() : 0.0);
		return new PingResult(target, Math.Round(times.Average(), 1), Math.Round(value, 1), (count - times.Count) * 100 / count);
	}

	public async Task<SpeedResult> SpeedTestAsync(IProgress<string>? progress, CancellationToken ct = default(CancellationToken))
	{
		try
		{
			progress?.Report(loc["network.speed.latency"]);
			double latency = await MeasureHttpLatencyAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
			progress?.Report(loc["network.speed.download"]);
			double download = await MeasureDownloadAsync(progress, ct).ConfigureAwait(continueOnCapturedContext: false);
			progress?.Report(loc["network.speed.upload"]);
			double value = await MeasureUploadAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
			return new SpeedResult(download, value, latency, null);
		}
		catch (Exception ex) when ((ex is HttpRequestException || ex is TaskCanceledException || ex is IOException) ? true : false)
		{
			if (ct.IsCancellationRequested)
			{
				throw;
			}
			return new SpeedResult(null, null, null, loc["network.speed.failed"]);
		}
	}

	private static async Task<double> MeasureHttpLatencyAsync(CancellationToken ct)
	{
		List<double> samples = new List<double>();
		for (int i = 0; i < 5; i++)
		{
			Stopwatch watch = Stopwatch.StartNew();
			using (await Http.GetAsync("https://speed.cloudflare.com/__down?bytes=0", HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(continueOnCapturedContext: false))
			{
				watch.Stop();
				samples.Add(watch.Elapsed.TotalMilliseconds);
			}
		}
		return Math.Round(samples.Skip(1).Min(), 1);
	}

	private static async Task<double> MeasureDownloadAsync(IProgress<string>? progress, CancellationToken ct)
	{
		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeout.CancelAfter(PhaseLimit);
		using HttpResponseMessage response = await Http.GetAsync($"{"https://speed.cloudflare.com"}/__down?bytes={52428800L}", HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(continueOnCapturedContext: false);
		response.EnsureSuccessStatusCode();
		double result;
		await using (Stream stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(continueOnCapturedContext: false))
		{
			byte[] buffer = new byte[81920];
			long total = 0L;
			Stopwatch watch = Stopwatch.StartNew();
			TimeSpan lastReport = TimeSpan.Zero;
			try
			{
				int num;
				while ((num = await stream.ReadAsync(buffer, timeout.Token).ConfigureAwait(continueOnCapturedContext: false)) > 0)
				{
					total += num;
					if (watch.Elapsed - lastReport > TimeSpan.FromMilliseconds(400.0))
					{
						lastReport = watch.Elapsed;
						progress?.Report($"↓ {ToMbps(total, watch.Elapsed):0.0} Mbps");
					}
				}
			}
			catch (OperationCanceledException) when (!ct.IsCancellationRequested)
			{
			}
			result = Math.Round(ToMbps(total, watch.Elapsed), 1);
		}
		return result;
	}

	private static async Task<double> MeasureUploadAsync(CancellationToken ct)
	{
		byte[] array = new byte[15728640];
		Random.Shared.NextBytes(array);
		using ByteArrayContent content = new ByteArrayContent(array);
		Stopwatch watch = Stopwatch.StartNew();
		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeout.CancelAfter(PhaseLimit * 2.0);
		using (await Http.PostAsync("https://speed.cloudflare.com/__up", content, timeout.Token).ConfigureAwait(continueOnCapturedContext: false))
		{
			watch.Stop();
			return Math.Round(ToMbps(15728640L, watch.Elapsed), 1);
		}
	}

	private static double ToMbps(long bytes, TimeSpan elapsed)
	{
		if (!(elapsed.TotalSeconds <= 0.0))
		{
			return (double)(bytes * 8) / elapsed.TotalSeconds / 1000000.0;
		}
		return 0.0;
	}

	public async Task<OperationResult> ResetNetworkAsync(IProgress<string>? progress, CancellationToken ct = default(CancellationToken))
	{
		(string, string)[] array = new (string, string)[4]
		{
			("netsh.exe", "winsock reset"),
			("netsh.exe", "int ip reset"),
			("netsh.exe", "int ipv6 reset"),
			("ipconfig.exe", "/flushdns")
		};
		List<string> failures = new List<string>();
		(string File, string Arguments)[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			var (text, arguments) = array2[i];
			progress?.Report(Path.GetFileNameWithoutExtension(text) + " " + arguments);
			ProcessResult processResult = await StreamingProcess.RunAsync(text, arguments, null, null, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (!processResult.Success && !arguments.StartsWith("int ip", StringComparison.Ordinal))
			{
				failures.Add(arguments + ": " + processResult.Combined.Trim());
			}
		}
		return (failures.Count == 0) ? OperationResult.Ok : OperationResult.Fail(string.Join(Environment.NewLine, failures));
	}
}
