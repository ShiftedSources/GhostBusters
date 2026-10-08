using System;

namespace GhostEye.Infrastructure.Probes;

public sealed record GpuAdapter
{
	public required string Name { get; init; }

	public required string ShortName { get; init; }

	public string? DriverVersion { get; init; }

	public DateTime? DriverDate { get; init; }

	public long MemoryBytes { get; init; }

	public bool IsDiscrete { get; init; }
}
