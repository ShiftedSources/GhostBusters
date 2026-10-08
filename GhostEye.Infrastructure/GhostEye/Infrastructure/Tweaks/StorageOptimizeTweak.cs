using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class StorageOptimizeTweak(IProcessRunner processRunner, ILocalizer loc) : ITweakAction
{
	public string Id => "perf.storage-optimize";

	public async Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return (await IsTrimDisabledAsync(ct).ConfigureAwait(continueOnCapturedContext: false))?.ToString();
	}

	public async Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		return await IsTrimDisabledAsync(ct).ConfigureAwait(continueOnCapturedContext: false) == false;
	}

	public async Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		bool? flag = await IsTrimDisabledAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		TweakApplicability result;
		if (flag.HasValue)
		{
			result = ((flag == true) ? TweakApplicability.Applicable : TweakApplicability.No(loc["applicability.trimAlreadyOn"]));
		}
		else
		{
			result = TweakApplicability.No(loc["applicability.trimUnreadable"]);
		}
		return result;
	}

	public async Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		bool? flag = await IsTrimDisabledAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!flag.HasValue)
		{
			return TweakResult.Fail(Id, loc["result.storage.queryFailed"]);
		}
		if (flag == false)
		{
			return TweakResult.Fail(Id, loc["result.storage.alreadyOn"]);
		}
		return (await processRunner.RunAsync("fsutil.exe", "behavior set DisableDeleteNotify 0", ct).ConfigureAwait(continueOnCapturedContext: false)).Success ? TweakResult.Ok(Id, "1", loc["result.storage.enabled"]) : TweakResult.Fail(Id, loc["result.storage.enableFailed"]);
	}

	public async Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		if (previousValue == "1")
		{
			return await Task.FromResult(TweakResult.Ok(Id, previousValue, loc["result.storage.keptOn"]));
		}
		return TweakResult.Ok(Id, previousValue, loc["result.storage.nothing"]);
	}

	private async Task<bool?> IsTrimDisabledAsync(CancellationToken ct)
	{
		ProcessResult processResult = await processRunner.RunAsync("fsutil.exe", "behavior query DisableDeleteNotify", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!processResult.Success)
		{
			return null;
		}
		List<int> list = (from line in processResult.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			select line.Split('=').LastOrDefault()?.Trim() into value
			where int.TryParse(value, out var _)
			select value).Select(int.Parse).ToList();
		return (list.Count == 0) ? ((bool?)null) : new bool?(list.Any((int v) => v != 0));
	}
}
