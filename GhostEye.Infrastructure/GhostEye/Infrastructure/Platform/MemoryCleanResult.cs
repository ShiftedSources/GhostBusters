namespace GhostEye.Infrastructure.Platform;

public sealed record MemoryCleanResult(bool Success, long FreedBytes, string Message);
