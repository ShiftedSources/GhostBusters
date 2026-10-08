using System.Collections.Generic;

namespace GhostEye.Infrastructure.Platform;

public sealed record BoostOptions(bool SwitchPowerPlan, bool PauseUpdates, bool FreeMemory, IReadOnlyCollection<string> AppsToClose)
{
	public string? AutoGameName { get; init; }

	public int? AutoProcessId { get; init; }
}
