namespace GhostEye.Infrastructure.Games;

public sealed record FrameStats(int Frames, double Seconds, double AverageFps, double OnePercentLowFps, double PointOnePercentLowFps, double AverageFrameTimeMs);
