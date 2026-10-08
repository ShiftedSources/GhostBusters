using System.Collections.Generic;
using System.Linq;
using GhostEye.Core.Models;

namespace GhostEye.Core.Abstractions;

public sealed record ApplyOutcome
{
	public required IReadOnlyList<TweakResult> Results { get; init; }

	public ChangeSet? ChangeSet { get; init; }

	public string? RestorePointWarning { get; init; }

	public bool RestartRequired => Results.Any((TweakResult r) => (object)r != null && r.Success && r.RestartRequired);

	public int SucceededCount => Results.Count((TweakResult r) => r.Success);

	public int FailedCount => Results.Count((TweakResult r) => !r.Success);
}
