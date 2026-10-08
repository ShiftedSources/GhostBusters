using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;

namespace GhostEye.Core.Services;

public sealed class SystemAnalyzer : ISystemAnalyzer
{
	public const int WarningPenalty = 6;

	public const int CriticalPenalty = 15;

	private readonly IReadOnlyList<IAnalysisProbe> _probes;

	private readonly ILocalizer _loc;

	private readonly IAppLogger? _logger;

	public SystemAnalyzer(IEnumerable<IAnalysisProbe> probes, ILocalizer loc, IAppLogger? logger = null)
	{
		_probes = probes.ToList();
		_loc = loc;
		_logger = logger;
	}

	public static int ComputeScore(IEnumerable<AnalysisItem> items)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		int num = 0;
		foreach (AnalysisItem item in items)
		{
			int num2 = num;
			num = num2 + item.Status switch
			{
				AnalysisStatus.Warning => 6, 
				AnalysisStatus.Critical => 15, 
				_ => 0, 
			};
		}
		return Math.Clamp(100 - num, 0, 100);
	}

	public async Task<AnalysisReport> AnalyzeAsync(IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		List<AnalysisItem> items = new List<AnalysisItem>(_probes.Count);
		foreach (IAnalysisProbe probe in _probes)
		{
			ct.ThrowIfCancellationRequested();
			progress?.Report(probe.Id);
			try
			{
				List<AnalysisItem> list = items;
				list.Add(await probe.RunAsync(ct).ConfigureAwait(continueOnCapturedContext: false));
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception exception)
			{
				_logger?.Error("Probe '" + probe.Id + "' failed", exception);
				items.Add(new AnalysisItem
				{
					Id = probe.Id,
					Group = probe.Group,
					Name = probe.Id,
					Description = _loc["probe.failed"],
					Status = AnalysisStatus.Warning
				});
			}
		}
		return new AnalysisReport
		{
			Items = items,
			Score = ComputeScore(items),
			CompletedAt = DateTimeOffset.Now
		};
	}
}
