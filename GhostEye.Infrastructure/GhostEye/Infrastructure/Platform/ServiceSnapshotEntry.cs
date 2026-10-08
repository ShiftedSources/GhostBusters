namespace GhostEye.Infrastructure.Platform;

public sealed record ServiceSnapshotEntry(string Name, ServiceStartupMode StartupMode, bool WasRunning);
