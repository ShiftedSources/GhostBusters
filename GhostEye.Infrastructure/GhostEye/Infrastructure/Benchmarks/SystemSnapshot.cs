using System;

namespace GhostEye.Infrastructure.Benchmarks;

public sealed record SystemSnapshot
{
	public required DateTimeOffset TakenAt { get; init; }

	public double? BootSeconds { get; init; }

	public long? UsedMemoryBytes { get; init; }

	public long? TotalMemoryBytes { get; init; }

	public int? StartupAppCount { get; init; }

	public double? BackgroundCpuPercent { get; init; }
}
