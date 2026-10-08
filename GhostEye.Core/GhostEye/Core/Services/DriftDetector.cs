using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;

namespace GhostEye.Core.Services;

public sealed class DriftDetector(IChangeHistoryStore history, IOptimizationEngine engine)
{
	public async Task<IReadOnlyList<string>> GetActiveTweakIdsAsync(CancellationToken ct = default(CancellationToken))
	{
		return (from r in (await history.GetAllAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).SelectMany((ChangeSet c) => c.Changes)
			where !r.Reverted
			select r.TweakId into id
			where (object)TweakCatalog.Find(id) != null
			select id).Distinct(StringComparer.Ordinal).ToList();
	}

	public async Task<DriftReport> CheckAsync(CancellationToken ct = default(CancellationToken))
	{
		IReadOnlyList<string> active = await GetActiveTweakIdsAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		if (active.Count == 0)
		{
			return DriftReport.Empty;
		}
		IReadOnlyDictionary<string, bool> states = await engine.GetAppliedStatesAsync(active, ct).ConfigureAwait(continueOnCapturedContext: false);
		List<string> driftedTweakIds = active.Where((string id) => states.TryGetValue(id, out var value) && !value).ToList();
		return new DriftReport(active, driftedTweakIds);
	}
}
