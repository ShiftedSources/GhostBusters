using System;
using System.Collections.Generic;

namespace GhostEye.App.Services;

public sealed record AppSettings
{
	public string Language { get; init; } = "en";

	public bool SnowEnabled { get; init; } = true;

	public bool NotificationsEnabled { get; init; } = true;

	public bool AutoRestorePoint { get; init; } = true;

	public bool StartWithWindows { get; init; }

	public string PrimaryDns { get; init; } = "1.1.1.1";

	public string SecondaryDns { get; init; } = "1.0.0.1";

	public bool RunInBackground { get; init; }

	public bool BackgroundHintShown { get; init; }

	public IReadOnlyList<string> DriftDismissed { get; init; } = Array.Empty<string>();

	public DateTimeOffset? LastDriftCheck { get; init; }

	public bool MaintenanceEnabled { get; init; }

	public int MaintenanceIntervalDays { get; init; } = 7;

	public DateTimeOffset? LastMaintenance { get; init; }

	public long LastMaintenanceFreedBytes { get; init; }

	public bool AutoBoostEnabled { get; init; }

	public IReadOnlyList<string> AutoBoostExcludedGames { get; init; } = Array.Empty<string>();

	public bool HalloweenTheme { get; init; }

	public IReadOnlyList<string> CustomGames { get; init; } = Array.Empty<string>();
}
