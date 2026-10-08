using System;
using System.Globalization;

namespace GhostEye.App.Services;

public static class GhostFormat
{
	private static CultureInfo Culture => AppServices.Localizer.Culture;

	public static string Number(double value, int decimals = 1)
	{
		return value.ToString("F" + decimals, Culture);
	}

	public static string Integer(long value)
	{
		return value.ToString(Culture);
	}

	public static string Percent(double value)
	{
		return Math.Round(value).ToString(Culture) + "%";
	}

	public static string Bytes(long bytes)
	{
		if (bytes >= 1048576)
		{
			if (bytes >= 1073741824)
			{
				return Number((double)bytes / 1073741824.0) + " GB";
			}
			return Number((double)bytes / 1048576.0, 0) + " MB";
		}
		if (bytes < 1024)
		{
			return $"{bytes} B";
		}
		return Number((double)bytes / 1024.0, 0) + " KB";
	}

	public static string Seconds(double seconds)
	{
		if (!(seconds >= 90.0))
		{
			return Number(seconds) + " s";
		}
		return Number(seconds / 60.0) + " min";
	}

	public static string Milliseconds(double ms)
	{
		return Math.Round(ms).ToString(Culture) + " ms";
	}

	public static string Date(DateTimeOffset value)
	{
		return value.ToString("d", Culture);
	}

	public static string Time(DateTimeOffset value)
	{
		return value.ToString("t", Culture);
	}

	public static string DateTime(DateTimeOffset value)
	{
		return Date(value) + " " + Time(value);
	}
}
