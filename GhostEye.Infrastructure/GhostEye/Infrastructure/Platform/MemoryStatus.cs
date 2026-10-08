namespace GhostEye.Infrastructure.Platform;

public readonly record struct MemoryStatus(long TotalBytes, long AvailableBytes)
{
	public long UsedBytes => TotalBytes - AvailableBytes;

	public double UsedPercent
	{
		get
		{
			if (TotalBytes != 0L)
			{
				return 100.0 * (double)UsedBytes / (double)TotalBytes;
			}
			return 0.0;
		}
	}
}
