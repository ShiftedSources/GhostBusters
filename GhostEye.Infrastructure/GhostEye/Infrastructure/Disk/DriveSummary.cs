namespace GhostEye.Infrastructure.Disk;

public sealed record DriveSummary(string Root, string Label, long TotalBytes, long FreeBytes)
{
	public long UsedBytes => TotalBytes - FreeBytes;
}
