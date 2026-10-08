namespace GhostEye.Infrastructure.Health;

public sealed record MonitorSample
{
	public double CpuPercent { get; init; }

	public double MemoryPercent { get; init; }

	public long MemoryUsedBytes { get; init; }

	public long MemoryTotalBytes { get; init; }

	public double? GpuPercent { get; init; }

	public double DiskPercent { get; init; }

	public double NetworkBitsPerSecond { get; init; }

	public double? CpuTemperatureC { get; init; }

	public double? GpuTemperatureC { get; init; }

	public string? GpuName { get; init; }
}
