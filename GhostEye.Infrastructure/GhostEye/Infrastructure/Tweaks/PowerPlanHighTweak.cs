using System;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class PowerPlanHighTweak(IPowerPlanService powerPlans, ILocalizer loc) : ITweakAction
{
	public string Id => "perf.power-plan-high";

	public Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(powerPlans.GetActiveScheme()?.ToString());
	}

	public Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(powerPlans.GetActiveScheme() == powerPlans.HighPerformanceGuid);
	}

	public Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(powerPlans.IsHighPerformanceAvailable() ? TweakApplicability.Applicable : TweakApplicability.No(loc["applicability.powerPlanMissing"]));
	}

	public Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		Guid? activeScheme = powerPlans.GetActiveScheme();
		if (!activeScheme.HasValue)
		{
			return Task.FromResult(TweakResult.Fail(Id, loc["result.powerPlan.readFailed"]));
		}
		if (!powerPlans.IsHighPerformanceAvailable())
		{
			return Task.FromResult(TweakResult.Fail(Id, loc["applicability.powerPlanMissing"]));
		}
		if (!powerPlans.TrySetActiveScheme(powerPlans.HighPerformanceGuid, out string error))
		{
			return Task.FromResult(TweakResult.Fail(Id, error));
		}
		string text = powerPlans.GetSchemeName(activeScheme.Value) ?? activeScheme.Value.ToString();
		return Task.FromResult(TweakResult.Ok(Id, activeScheme.Value.ToString(), loc.Format("result.powerPlan.applied", text)));
	}

	public Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		if (!Guid.TryParse(previousValue, out var result))
		{
			return Task.FromResult(TweakResult.Fail(Id, loc["result.powerPlan.noPrevious"]));
		}
		if (!powerPlans.TrySetActiveScheme(result, out string error))
		{
			return Task.FromResult(TweakResult.Fail(Id, error));
		}
		string text = powerPlans.GetSchemeName(result) ?? result.ToString();
		return Task.FromResult(TweakResult.Ok(Id, previousValue, loc.Format("result.powerPlan.reverted", text)));
	}
}
