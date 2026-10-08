using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;
using GhostEye.Infrastructure.Probes;

namespace GhostEye.Infrastructure.Benchmarks;

[SupportedOSPlatform("windows")]
public sealed class SnapshotService(IStartupManagerService startup, IAppLogger logger, ILocalizer loc, string baselinePath) : ISnapshotService
{
	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	public async Task<SystemSnapshot> CaptureAsync(CancellationToken ct = default(CancellationToken))
	{
		(ulong, ulong)? tuple = RamProbe.Read();
		SystemSnapshot systemSnapshot = new SystemSnapshot
		{
			TakenAt = DateTimeOffset.Now,
			BootSeconds = ReadBootDuration(),
			UsedMemoryBytes = ((!tuple.HasValue) ? ((long?)null) : new long?((long)tuple.Value.Item1)),
			TotalMemoryBytes = ((!tuple.HasValue) ? ((long?)null) : new long?((long)tuple.Value.Item2)),
			StartupAppCount = CountStartupApps(),
			BackgroundCpuPercent = await SampleCpuAsync(loc, ct).ConfigureAwait(continueOnCapturedContext: false)
		};
		return systemSnapshot;
	}

	public SystemSnapshot? LoadBaseline()
	{
		if (!File.Exists(baselinePath))
		{
			return null;
		}
		try
		{
			return JsonSerializer.Deserialize<SystemSnapshot>(File.ReadAllText(baselinePath), Options);
		}
		catch (Exception ex) when ((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			logger.Warn("Baseline snapshot unreadable.");
			return null;
		}
	}

	public void SaveBaseline(SystemSnapshot snapshot)
	{
		try
		{
			File.WriteAllText(baselinePath, JsonSerializer.Serialize(snapshot, Options));
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			logger.Warn("Could not save the baseline snapshot.");
		}
	}

	public void ClearBaseline()
	{
		try
		{
			if (File.Exists(baselinePath))
			{
				File.Delete(baselinePath);
			}
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
	}

	private double? ReadBootDuration()
	{
		try
		{
			using EventLogReader eventLogReader = new EventLogReader(new EventLogQuery("Microsoft-Windows-Diagnostics-Performance/Operational", PathType.LogName, "*[System/EventID=100]")
			{
				ReverseDirection = true
			});
			using EventRecord eventRecord = eventLogReader.ReadEvent();
			if (eventRecord == null)
			{
				return null;
			}
			string text = eventRecord.ToXml();
			string text2 = "Name='BootTime'>";
			int num = text.IndexOf(text2, StringComparison.Ordinal);
			if (num < 0)
			{
				return null;
			}
			num += text2.Length;
			int num2 = text.IndexOf('<', num);
			int num3 = num;
			double result;
			return double.TryParse(text.Substring(num3, num2 - num3), out result) ? new double?(result / 1000.0) : ((double?)null);
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is EventLogException) ? true : false)
		{
			return null;
		}
	}

	private int? CountStartupApps()
	{
		try
		{
			return startup.GetEntries().Count((StartupEntry e) => !e.IsDisabled);
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is SecurityException) ? true : false)
		{
			return null;
		}
	}

	private static async Task<double?> SampleCpuAsync(ILocalizer loc, CancellationToken ct)
	{
		using CpuProbe probe = new CpuProbe(loc);
		List<double> samples = new List<double>();
		for (int i = 0; i < 4; i++)
		{
			await Task.Delay(250, ct).ConfigureAwait(continueOnCapturedContext: false);
			double? num = probe.Sample();
			if (num.HasValue)
			{
				double valueOrDefault = num.GetValueOrDefault();
				samples.Add(valueOrDefault);
			}
		}
		return (samples.Count == 0) ? ((double?)null) : new double?(samples.Average());
	}
}
