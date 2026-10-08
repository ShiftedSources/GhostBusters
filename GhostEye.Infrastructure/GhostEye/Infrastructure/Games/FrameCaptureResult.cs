namespace GhostEye.Infrastructure.Games;

public sealed record FrameCaptureResult(bool Success, FrameStats? Stats, string Error);
