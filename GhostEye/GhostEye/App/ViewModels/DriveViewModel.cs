using GhostEye.App.Services;
using GhostEye.Infrastructure.Disk;

namespace GhostEye.App.ViewModels;

public sealed class DriveViewModel(DriveSummary drive)
{
	public DriveSummary Drive { get; } = drive;

	public string Label => Drive.Label;

	public double UsedRatio
	{
		get
		{
			if (Drive.TotalBytes != 0L)
			{
				return (double)Drive.UsedBytes / (double)Drive.TotalBytes;
			}
			return 0.0;
		}
	}

	public string UsageLabel => AppServices.Localizer.Format("disk.drive.usage", GhostFormat.Bytes(Drive.FreeBytes), GhostFormat.Bytes(Drive.TotalBytes));

	public bool IsLow => Drive.FreeBytes < Drive.TotalBytes / 10;
}
