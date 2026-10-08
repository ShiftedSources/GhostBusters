using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class UltimatePlanTweak(IPowerPlanService powerPlans, IProcessRunner processRunner, IRegistryService registry, ILocalizer loc) : ITweakAction
{
	private static readonly Guid Template = new Guid("e9a42b02-d5df-448d-aa00-03f14749eb61");

	private const string StateKey = "HKCU\\Software\\GhostEye";

	private const string StateValue = "UltimatePlanGuid";

	public string Id => "perf.ultimate-plan";

	public Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(powerPlans.GetActiveScheme()?.ToString());
	}

	public Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		Guid? activeScheme = powerPlans.GetActiveScheme();
		return Task.FromResult(activeScheme.HasValue && (activeScheme == Template || activeScheme == FindExistingPlan()));
	}

	public Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(TweakApplicability.Applicable);
	}

	public async Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		Guid? previous = powerPlans.GetActiveScheme();
		if (!previous.HasValue)
		{
			return TweakResult.Fail(Id, loc["result.powerPlan.readFailed"]);
		}
		Guid? guid = FindExistingPlan();
		if (!guid.HasValue)
		{
			ProcessResult processResult = await processRunner.RunAsync("powercfg.exe", $"-duplicatescheme {Template}", ct).ConfigureAwait(continueOnCapturedContext: false);
			Match match = GuidPattern().Match(processResult.StandardOutput);
			if (!processResult.Success || !match.Success || !Guid.TryParse(match.Value, out var created))
			{
				return TweakResult.Fail(Id, loc.Format("result.ultimatePlan.createFailed", processResult.Combined.Trim()));
			}
			guid = created;
			RegistryService.TryRun(loc, () =>
			{
				registry.WriteString("HKCU\\Software\\GhostEye", "UltimatePlanGuid", created.ToString());
			}, out string _);
		}
		if (!powerPlans.TrySetActiveScheme(guid.Value, out string error2))
		{
			return TweakResult.Fail(Id, error2);
		}
		string text = powerPlans.GetSchemeName(previous.Value) ?? previous.Value.ToString();
		return TweakResult.Ok(Id, previous.Value.ToString(), loc.Format("result.ultimatePlan.applied", text));
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

	private Guid? FindExistingPlan()
	{
		IReadOnlyList<PowerPlan> schemes = powerPlans.GetSchemes();
		if (schemes.Any((PowerPlan s) => s.Id == Template))
		{
			return Template;
		}
		if (!Guid.TryParse(registry.ReadString("HKCU\\Software\\GhostEye", "UltimatePlanGuid"), out var remembered) || !schemes.Any((PowerPlan s) => s.Id == remembered))
		{
			return null;
		}
		return remembered;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex GuidPattern()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__GuidPattern_10.Instance;
	}
}
