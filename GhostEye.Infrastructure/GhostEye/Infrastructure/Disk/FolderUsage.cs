namespace GhostEye.Infrastructure.Disk;

public sealed record FolderUsage(string Path, string Name, long Bytes, long Files, bool IsFilesEntry, bool IsInaccessible);
