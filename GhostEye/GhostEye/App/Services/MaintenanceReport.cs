namespace GhostEye.App.Services;

public sealed record MaintenanceReport(long FreedBytes, int DeletedFiles, int DriftedTweaks);
