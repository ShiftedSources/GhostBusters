using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Games;

public sealed class GameLibrary(IRegistryService registry)
{
	public IReadOnlyList<GameEntry> Detect(IEnumerable<string>? customExecutables = null)
	{
		List<GameEntry> games = new List<GameEntry>();
		Collect(Steam);
		Collect(Epic);
		Collect(Gog);
		Collect(Ubisoft);
		Collect(Xbox);
		foreach (string item in customExecutables ?? Array.Empty<string>())
		{
			if (File.Exists(item))
			{
				games.Add(new GameEntry(item, Path.GetFileNameWithoutExtension(item), "Custom", item, IsExecutable: true));
			}
		}
		return (from g in games.Where((GameEntry g) => g.IsExecutable || Directory.Exists(g.InstallPath)).GroupBy((GameEntry g) => g.Id, StringComparer.OrdinalIgnoreCase)
			select g.First()).OrderBy((GameEntry g) => g.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
		void Collect(Func<IEnumerable<GameEntry>> source)
		{
			try
			{
				games.AddRange(source());
			}
			catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException) ? true : false)
			{
			}
		}
	}

	private static string Normalise(string path)
	{
		return Path.GetFullPath(path).TrimEnd('\\');
	}

	private IEnumerable<GameEntry> Steam()
	{
		string text = registry.ReadString("HKCU\\Software\\Valve\\Steam", "SteamPath");
		if (string.IsNullOrWhiteSpace(text))
		{
			yield break;
		}
		List<string> list = new List<string> { text.Replace('/', '\\') };
		string path = Path.Combine(list[0], "steamapps", "libraryfolders.vdf");
		if (File.Exists(path))
		{
			foreach (Match item in VdfPath().Matches(File.ReadAllText(path)))
			{
				list.Add(item.Groups[1].Value.Replace("\\\\", "\\"));
			}
		}
		foreach (string item2 in list.Distinct(StringComparer.OrdinalIgnoreCase))
		{
			string steamapps = Path.Combine(item2, "steamapps");
			if (!Directory.Exists(steamapps))
			{
				continue;
			}
			foreach (string item3 in Directory.EnumerateFiles(steamapps, "appmanifest_*.acf"))
			{
				string text2 = File.ReadAllText(item3);
				string text3 = AcfValue(text2, "name");
				string text4 = AcfValue(text2, "installdir");
				if (text3 != null && text4 != null && !text3.StartsWith("Steamworks", StringComparison.OrdinalIgnoreCase) && !text3.Contains("Redistributable", StringComparison.OrdinalIgnoreCase))
				{
					string text5 = Normalise(Path.Combine(steamapps, "common", text4));
					yield return new GameEntry(text5, text3, "Steam", text5);
				}
			}
		}
	}

	private static string? AcfValue(string text, string key)
	{
		Match match = Regex.Match(text, "\"" + Regex.Escape(key) + "\"\\s+\"([^\"]*)\"", RegexOptions.IgnoreCase);
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[1].Value;
	}

	private static IEnumerable<GameEntry> Epic()
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic\\EpicGamesLauncher\\Data\\Manifests");
		if (!Directory.Exists(path))
		{
			yield break;
		}
		foreach (string item in Directory.EnumerateFiles(path, "*.item"))
		{
			using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(item));
			JsonElement rootElement = doc.RootElement;
			if (rootElement.TryGetProperty("InstallLocation", out var value))
			{
				string text = value.GetString();
				if (text != null && text.Length > 0)
				{
					string text2 = (rootElement.TryGetProperty("DisplayName", out var value2) ? value2.GetString() : null);
					string text3 = Normalise(text);
					yield return new GameEntry(text3, text2 ?? Path.GetFileName(text3), "Epic", text3);
				}
			}
		}
	}

	private IEnumerable<GameEntry> Gog()
	{
		foreach (string subKeyName in registry.GetSubKeyNames("HKLM32\\SOFTWARE\\GOG.com\\Games"))
		{
			string text = registry.ReadString("HKLM32\\SOFTWARE\\GOG.com\\Games\\" + subKeyName, "path");
			if (!string.IsNullOrWhiteSpace(text))
			{
				string text2 = Normalise(text);
				yield return new GameEntry(text2, registry.ReadString("HKLM32\\SOFTWARE\\GOG.com\\Games\\" + subKeyName, "gameName") ?? Path.GetFileName(text2), "GOG", text2);
			}
		}
	}

	private IEnumerable<GameEntry> Ubisoft()
	{
		foreach (string subKeyName in registry.GetSubKeyNames("HKLM32\\SOFTWARE\\Ubisoft\\Launcher\\Installs"))
		{
			string text = registry.ReadString("HKLM32\\SOFTWARE\\Ubisoft\\Launcher\\Installs\\" + subKeyName, "InstallDir");
			if (!string.IsNullOrWhiteSpace(text))
			{
				string text2 = Normalise(text.Replace('/', '\\'));
				yield return new GameEntry(text2, Path.GetFileName(text2), "Ubisoft", text2);
			}
		}
	}

	private static IEnumerable<GameEntry> Xbox()
	{
		foreach (DriveInfo item in from d in DriveInfo.GetDrives()
			where d.DriveType == DriveType.Fixed && d.IsReady
			select d)
		{
			string path = Path.Combine(item.RootDirectory.FullName, "XboxGames");
			if (!Directory.Exists(path))
			{
				continue;
			}
			foreach (string item2 in Directory.EnumerateDirectories(path))
			{
				string fileName = Path.GetFileName(item2);
				if (!fileName.Equals("GameSave", StringComparison.OrdinalIgnoreCase))
				{
					string text = Normalise(item2);
					yield return new GameEntry(text, fileName, "Xbox", text);
				}
			}
		}
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex VdfPath()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__VdfPath_3.Instance;
	}
}
