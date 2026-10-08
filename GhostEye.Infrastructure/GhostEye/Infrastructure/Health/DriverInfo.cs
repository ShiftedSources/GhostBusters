using System;

namespace GhostEye.Infrastructure.Health;

public sealed record DriverInfo(string DeviceName, string DeviceClass, string Manufacturer, string Version, DateTime? Date, string? DownloadUrl)
{
	public bool IsInbox
	{
		get
		{
			if (!Manufacturer.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) && !Manufacturer.StartsWith('(') && !Manufacturer.StartsWith("Standard", StringComparison.OrdinalIgnoreCase))
			{
				return Manufacturer.StartsWith("Generic", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}

	public bool IsOld
	{
		get
		{
			if (!IsInbox)
			{
				DateTime? date = Date;
				if (date.HasValue)
				{
					DateTime valueOrDefault = date.GetValueOrDefault();
					return DateTime.Now - valueOrDefault > TimeSpan.FromDays(730.0);
				}
			}
			return false;
		}
	}

	public const int OldAfterYears = 2;
}
