using System;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

public sealed class PowerPlanProbe(IPowerPlanService powerPlans, ILocalizer loc) : IAnalysisProbe
{
	public string Id => "perf.power-plan";

	public AnalysisGroup Group => AnalysisGroup.Performance;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		Guid? activeScheme = powerPlans.GetActiveScheme();
		if (!activeScheme.HasValue)
		{
			return Task.FromResult(new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.power.name"],
				Description = loc["probe.power.unreadable"],
				Status = AnalysisStatus.Warning
			});
		}
		string text = powerPlans.GetSchemeName(activeScheme.Value) ?? activeScheme.Value.ToString();
		bool flag = activeScheme.Value == powerPlans.HighPerformanceGuid;
		return Task.FromResult(new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.power.name"],
			Measurement = text,
			Status = ((!flag) ? AnalysisStatus.Warning : AnalysisStatus.Ok),
			Description = (flag ? loc["probe.power.high"] : loc.Format("probe.power.other", text)),
			SuggestedTweakId = (flag ? null : "perf.power-plan-high")
		});
	}
}
