using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Models;

namespace GhostEye.Core.Abstractions;

public interface IOptimizationEngine
{
	IReadOnlyList<TweakDefinition> Catalog { get; }

	Task<IReadOnlyDictionary<string, string?>> ReadCurrentValuesAsync(IEnumerable<string> tweakIds, CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyDictionary<string, TweakApplicability>> GetApplicabilityAsync(IEnumerable<string> tweakIds, CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyDictionary<string, bool>> GetAppliedStatesAsync(IEnumerable<string> tweakIds, CancellationToken ct = default(CancellationToken));

	Task<ApplyOutcome> ApplyAsync(ApplyRequest request, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));

	Task<ApplyOutcome> RevertChangeSetAsync(ChangeSet changeSet, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));

	Task<ApplyOutcome> RevertTweaksAsync(IReadOnlyList<string> tweakIds, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));

	Task<ApplyOutcome> ReapplyTweaksAsync(IReadOnlyList<string> tweakIds, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));
}
