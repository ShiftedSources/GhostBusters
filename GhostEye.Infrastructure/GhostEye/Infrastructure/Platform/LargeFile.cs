using System;

namespace GhostEye.Infrastructure.Platform;

public sealed record LargeFile(string Path, long Bytes, DateTime LastWrite);
