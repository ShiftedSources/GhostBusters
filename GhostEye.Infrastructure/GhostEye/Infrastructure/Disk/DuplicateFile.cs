using System;

namespace GhostEye.Infrastructure.Disk;

public sealed record DuplicateFile(string Path, DateTime LastWrite);
