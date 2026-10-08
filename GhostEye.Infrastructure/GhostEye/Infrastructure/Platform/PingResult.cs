namespace GhostEye.Infrastructure.Platform;

public sealed record PingResult(PingTarget Target, double? AverageMs, double? JitterMs, int LossPercent);
