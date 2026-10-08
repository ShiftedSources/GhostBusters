using System;

namespace GhostEye.Infrastructure.Apps;

public sealed record InstalledApp
{
	public required string Key { get; init; }

	public required string Name { get; init; }

	public string Publisher { get; init; } = string.Empty;

	public string Version { get; init; } = string.Empty;

	public long SizeBytes { get; init; }

	public DateTime? InstalledOn { get; init; }

	public DateTime? LastUsed { get; init; }

	public string InstallLocation { get; init; } = string.Empty;

	public required string UninstallCommand { get; init; }

	public string? QuietUninstallCommand { get; init; }

	public bool IsMachineWide { get; init; }

	public string? IconPath { get; init; }
}
