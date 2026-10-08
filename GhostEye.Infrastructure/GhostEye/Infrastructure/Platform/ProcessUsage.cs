namespace GhostEye.Infrastructure.Platform;

public sealed record ProcessUsage(int Id, string Name, string? Path, long WorkingSetBytes, double CpuPercent);
