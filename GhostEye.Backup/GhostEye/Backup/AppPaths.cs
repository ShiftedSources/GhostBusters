using System;
using System.IO;

namespace GhostEye.Backup;

public static class AppPaths
{
	public static string Root { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GhostEye");

	public static string Backups => EnsureDirectory(Path.Combine(Root, "backups"));

	public static string Logs => EnsureDirectory(Path.Combine(Root, "logs"));

	public static string Cache => EnsureDirectory(Path.Combine(Root, "cache"));

	public static string HistoryFile => Path.Combine(EnsureDirectory(Root), "history.json");

	public static string SettingsFile => Path.Combine(EnsureDirectory(Root), "settings.json");

	private static string EnsureDirectory(string path)
	{
		Directory.CreateDirectory(path);
		return path;
	}
}
