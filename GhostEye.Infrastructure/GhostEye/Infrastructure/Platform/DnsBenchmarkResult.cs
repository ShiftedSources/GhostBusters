namespace GhostEye.Infrastructure.Platform;

public sealed record DnsBenchmarkResult(string Name, string Address, string? Secondary, double? MedianMs, int LossPercent, bool IsCurrent);
