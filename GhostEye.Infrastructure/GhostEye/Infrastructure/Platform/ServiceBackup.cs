using System;
using System.Collections.Generic;

namespace GhostEye.Infrastructure.Platform;

public sealed record ServiceBackup(string Id, DateTime CreatedAt, bool IsAutomatic, IReadOnlyList<ServiceSnapshotEntry> Services, IReadOnlyList<TaskSnapshotEntry> Tasks);
