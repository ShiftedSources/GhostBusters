namespace GhostEye.Infrastructure.Health;

public sealed record SecurityCheck(string Key, HealthLevel Level, string Detail);
