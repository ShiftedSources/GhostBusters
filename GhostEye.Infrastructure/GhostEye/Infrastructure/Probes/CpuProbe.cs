using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;

namespace GhostEye.Infrastructure.Probes;

[SupportedOSPlatform("windows")]
public sealed class CpuProbe(ILocalizer loc) : IAnalysisProbe, IDisposable
{
	private readonly PerformanceCounter? _counter = CreateCounter();

	public string Id => "perf.cpu";

	public AnalysisGroup Group => AnalysisGroup.Performance;

	private static PerformanceCounter? CreateCounter()
	{
		try
		{
			PerformanceCounter performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", readOnly: true);
			performanceCounter.NextValue();
			return performanceCounter;
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is UnauthorizedAccessException) ? true : false)
		{
			return null;
		}
	}

	public double? Sample()
	{
		if (_counter == null)
		{
			return null;
		}
		try
		{
			return _counter.NextValue();
		}
		catch (InvalidOperationException)
		{
			return null;
		}
	}

	public async Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		List<double> samples = new List<double>();
		for (int i = 0; i < 4; i++)
		{
			await Task.Delay(250, ct).ConfigureAwait(continueOnCapturedContext: false);
			double? num = Sample();
			if (num.HasValue)
			{
				double valueOrDefault = num.GetValueOrDefault();
				samples.Add(valueOrDefault);
			}
		}
		if (samples.Count == 0)
		{
			return new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.cpu.name"],
				Description = loc["probe.cpu.noCounters"],
				Status = AnalysisStatus.Warning
			};
		}
		double num2 = samples.Average();
		AnalysisStatus analysisStatus = ((num2 >= 40.0) ? AnalysisStatus.Critical : ((num2 >= 20.0) ? AnalysisStatus.Warning : AnalysisStatus.Ok));
		AnalysisStatus analysisStatus2 = analysisStatus;
		return new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.cpu.name"],
			Measurement = $"{Math.Round(num2)}%",
			Status = analysisStatus2,
			Description = ((analysisStatus2 == AnalysisStatus.Ok) ? loc["probe.cpu.ok"] : loc["probe.cpu.high"]),
			SuggestedTweakId = ((analysisStatus2 == AnalysisStatus.Ok) ? null : "perf.startup-cleanup")
		};
	}

	public void Dispose()
	{
		_counter?.Dispose();
	}
}
