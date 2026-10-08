namespace GhostEye.Infrastructure.Platform;

public sealed record SpeedResult(double? DownloadMbps, double? UploadMbps, double? LatencyMs, string? Error);
