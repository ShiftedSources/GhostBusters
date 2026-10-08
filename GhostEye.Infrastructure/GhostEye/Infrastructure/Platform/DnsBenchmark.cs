using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Infrastructure.Tweaks;

namespace GhostEye.Infrastructure.Platform;

public static class DnsBenchmark
{
	private static readonly string[] Domains = new string[5] { "google.com", "youtube.com", "microsoft.com", "amazon.com", "wikipedia.org" };

	private const int Rounds = 3;

	private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(1000.0);

	public static IReadOnlyList<(string Name, string Address, string? Secondary, bool IsCurrent)> Candidates(string currentLabel)
	{
		List<(string, string, string, bool)> list = new List<(string, string, string, bool)>();
		string text = CurrentResolver();
		if (text != null && (object)DnsPresets.FindByAddress(text) == null)
		{
			list.Add((currentLabel + " (" + text + ")", text, null, true));
		}
		foreach (DnsPreset item in DnsPresets.All)
		{
			list.Add((item.Name, item.Primary, item.Secondary, item.Primary == text));
		}
		return list;
	}

	public static async Task<IReadOnlyList<DnsBenchmarkResult>> RunAsync(string currentLabel, IProgress<int>? progress, CancellationToken ct = default(CancellationToken))
	{
		IReadOnlyList<(string Name, string Address, string? Secondary, bool IsCurrent)> candidates = Candidates(currentLabel);
		List<double>[] samples = candidates.Select(((string Name, string Address, string Secondary, bool IsCurrent) _) => new List<double>()).ToArray();
		int[] lost = new int[candidates.Count];
		int total = 3 * Domains.Length;
		int done = 0;
		for (int round = 0; round < 3; round++)
		{
			string[] domains = Domains;
			foreach (string domain in domains)
			{
				double?[] array = await Task.WhenAll(candidates.Select(((string Name, string Address, string Secondary, bool IsCurrent) c, int i) => (samples[i].Count != 0 || lost[i] < 3) ? QueryAsync(c.Address, domain, ct) : Task.FromResult<double?>(null))).ConfigureAwait(continueOnCapturedContext: false);
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					double? num3 = array[num2];
					if (num3.HasValue)
					{
						double valueOrDefault = num3.GetValueOrDefault();
						samples[num2].Add(valueOrDefault);
					}
					else
					{
						lost[num2]++;
					}
				}
				if (progress != null)
				{
					int num4 = done + 1;
					done = num4;
					progress.Report(num4 * 100 / total);
				}
			}
		}
		return (from r in candidates.Select(((string Name, string Address, string Secondary, bool IsCurrent) c, int i) => new DnsBenchmarkResult(c.Name, c.Address, c.Secondary, Median(samples[i]), lost[i] * 100 / total, c.IsCurrent))
			orderby r.MedianMs ?? double.MaxValue
			select r).ToList();
	}

	public static string? CurrentResolver()
	{
		try
		{
			return (from n in NetworkInterface.GetAllNetworkInterfaces()
				where n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
				select n.GetIPProperties() into p
				where p.GatewayAddresses.Any((GatewayIPAddressInformation g) => g.Address.AddressFamily == AddressFamily.InterNetwork && !g.Address.Equals(IPAddress.Any))
				select p).SelectMany((IPInterfaceProperties p) => p.DnsAddresses).FirstOrDefault((IPAddress a) => a.AddressFamily == AddressFamily.InterNetwork)?.ToString();
		}
		catch (NetworkInformationException)
		{
			return null;
		}
	}

	private static async Task<double?> QueryAsync(string server, string domain, CancellationToken ct)
	{
		ushort id = (ushort)Random.Shared.Next(65535);
		byte[] array = BuildQuery(id, domain);
		using UdpClient client = new UdpClient(AddressFamily.InterNetwork);
		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeout.CancelAfter(Timeout);
		try
		{
			Stopwatch watch = Stopwatch.StartNew();
			await client.SendAsync(array, new IPEndPoint(IPAddress.Parse(server), 53), timeout.Token).ConfigureAwait(continueOnCapturedContext: false);
			while (!IsAnswerTo((await client.ReceiveAsync(timeout.Token).ConfigureAwait(continueOnCapturedContext: false)).Buffer, id))
			{
			}
			return watch.Elapsed.TotalMilliseconds;
		}
		catch (OperationCanceledException) when (!ct.IsCancellationRequested)
		{
			return null;
		}
		catch (SocketException)
		{
			return null;
		}
	}

	internal static byte[] BuildQuery(ushort id, string domain)
	{
		List<byte> list = new List<byte>
		{
			(byte)(id >> 8),
			(byte)id,
			1,
			0,
			0,
			1,
			0,
			0,
			0,
			0,
			0,
			0
		};
		string[] array = domain.Split('.');
		foreach (string text in array)
		{
			list.Add((byte)text.Length);
			list.AddRange(Encoding.ASCII.GetBytes(text));
		}
		list.AddRange(new _003C_003Ez__ReadOnlyArray<byte>(new byte[5] { 0, 0, 1, 0, 1 }));
		return list.ToArray();
	}

	internal static bool IsAnswerTo(byte[] reply, ushort id)
	{
		bool flag = reply.Length >= 12 && reply[0] == (byte)(id >> 8) && reply[1] == (byte)id && (reply[2] & 0x80) != 0;
		if (flag)
		{
			int num = reply[3] & 0xF;
			bool flag2 = ((num == 0 || num == 3) ? true : false);
			flag = flag2;
		}
		return flag;
	}

	private static double? Median(List<double> values)
	{
		if (values.Count == 0)
		{
			return null;
		}
		values.Sort();
		int num = values.Count / 2;
		return (values.Count % 2 == 1) ? values[num] : ((values[num - 1] + values[num]) / 2.0);
	}
}
