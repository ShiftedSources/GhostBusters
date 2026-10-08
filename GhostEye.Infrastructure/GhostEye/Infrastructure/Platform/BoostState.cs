using System;
using System.Collections.Generic;

namespace GhostEye.Infrastructure.Platform;

public sealed record BoostState
{
	public DateTimeOffset StartedAt { get; init; }

	public Guid? PreviousPowerPlan { get; init; }

	public IReadOnlyList<string> StoppedServices { get; init; } = Array.Empty<string>();

	public IReadOnlyList<string> ClosedExecutables { get; init; } = Array.Empty<string>();

	public string? AutoGameName { get; init; }

	public int? AutoProcessId { get; init; }
}
