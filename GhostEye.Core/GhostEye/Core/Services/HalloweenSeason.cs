using System;

namespace GhostEye.Core.Services;

public static class HalloweenSeason
{
	private static readonly TimeZoneInfo? Rome = FindRome();

	public static bool IsSeason(DateTime romeDate)
	{
		if (romeDate.Month != 10)
		{
			if (romeDate.Month == 11)
			{
				return romeDate.Day <= 2;
			}
			return false;
		}
		return true;
	}

	public static bool IsSeasonAt(DateTime utc)
	{
		return IsSeason(ToRome(utc));
	}

	public static DateTime ToRome(DateTime utc)
	{
		utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
		if (Rome != null)
		{
			return TimeZoneInfo.ConvertTimeFromUtc(utc, Rome);
		}
		return utc.ToLocalTime();
	}

	public static DateTime NextRomeMidnightUtc(DateTime utc)
	{
		DateTime value = ToRome(utc).Date.AddDays(1.0);
		if (Rome != null)
		{
			return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), Rome);
		}
		return DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
	}

	private static TimeZoneInfo? FindRome()
	{
		string[] array = new string[2] { "Europe/Rome", "W. Europe Standard Time" };
		foreach (string id in array)
		{
			try
			{
				return TimeZoneInfo.FindSystemTimeZoneById(id);
			}
			catch (Exception ex) when ((ex is TimeZoneNotFoundException || ex is InvalidTimeZoneException) ? true : false)
			{
			}
		}
		return null;
	}
}
