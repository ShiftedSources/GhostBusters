using System.Collections.Generic;

namespace GhostEye.Infrastructure.Platform;

public sealed record BoostReport(bool Success, int ClosedApps, long FreedBytes, string? PowerPlan, IReadOnlyList<string> Warnings);
