using System.Collections.Generic;

namespace GhostEye.Infrastructure.Platform;

public sealed record ServiceRestoreResult(int Restored, IReadOnlyList<string> Failed);
