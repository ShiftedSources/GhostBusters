using System;
using System.IO;
using GhostEye.Backup;

namespace GhostEye.App.Services;

public static class PendingPage
{
	private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(2.0);

	private static string FilePath => Path.Combine(AppPaths.Root, "pending-page");

	public static void Write(string? page)
	{
		if (string.IsNullOrWhiteSpace(page))
		{
			return;
		}
		try
		{
			File.WriteAllText(FilePath, page);
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			AppServices.Logger.Info("Could not record the page to reopen: " + ex.Message);
		}
	}

	public static string? Take()
	{
		try
		{
			string filePath = FilePath;
			if (!File.Exists(filePath))
			{
				return null;
			}
			string text = ((DateTime.UtcNow - File.GetLastWriteTimeUtc(filePath) < MaxAge) ? File.ReadAllText(filePath).Trim() : null);
			File.Delete(filePath);
			return string.IsNullOrEmpty(text) ? null : text;
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			return null;
		}
	}

	public static void Clear()
	{
		try
		{
			File.Delete(FilePath);
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
	}
}
