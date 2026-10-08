using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Apps;

public sealed class UsageHistory
{
	private const string UserAssistRoot = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\UserAssist";

	private const string BamRoot = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\bam\\State\\UserSettings";

	private const int UserAssistTimeOffset = 60;

	private readonly Dictionary<string, DateTime> _byPath = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, DateTime> _byExeName = new Dictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);

	public int Count => _byPath.Count + _byExeName.Count;

	private UsageHistory()
	{
	}

	public static UsageHistory Collect(IRegistryService registry)
	{
		UsageHistory usageHistory = new UsageHistory();
		usageHistory.ReadUserAssist(registry);
		usageHistory.ReadBam(registry);
		usageHistory.ReadPrefetch();
		usageHistory.ReadRunningProcesses();
		return usageHistory;
	}

	public DateTime? LastUsed(IEnumerable<string> folders, IEnumerable<string> executables)
	{
		DateTime? latest = null;
		List<string> list = (from f in folders
			where !string.IsNullOrWhiteSpace(f)
			select f.TrimEnd('\\') + "\\").Where(IsSpecificFolder).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		if (list.Count > 0)
		{
			foreach (KeyValuePair<string, DateTime> item in _byPath)
			{
				var (path, when) = item;
				if (!IsHousekeeping(path) && list.Any((string p) => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
				{
					Consider(when);
				}
			}
		}
		foreach (string item2 in executables.Where((string e) => !string.IsNullOrWhiteSpace(e)))
		{
			if (!IsHousekeeping(item2))
			{
				if (_byPath.TryGetValue(item2, out var value))
				{
					Consider(value);
				}
				if (_byExeName.TryGetValue(Path.GetFileName(item2), out var value2))
				{
					Consider(value2);
				}
			}
		}
		return latest;
		void Consider(DateTime value3)
		{
			if (latest.HasValue)
			{
				DateTime? dateTime2 = latest;
				if (!(value3 > dateTime2))
				{
					return;
				}
			}
			latest = value3;
		}
	}

	private void ReadUserAssist(IRegistryService registry)
	{
		foreach (string item in Safe(() => registry.GetSubKeyNames("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\UserAssist")))
		{
			string countKey = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\UserAssist\\" + item + "\\Count";
			foreach (string encoded in Safe(() => registry.GetValueNames(countKey)))
			{
				DateTime? dateTime = ReadUserAssistTime(Safe(() => registry.ReadBinary(countKey, encoded)));
				if (!dateTime.HasValue)
				{
					continue;
				}
				string text = ExpandKnownFolder(Rot13(encoded));
				if (text == null)
				{
					continue;
				}
				if (text.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase))
				{
					string text2 = StartupCommand.ExtractExecutable(ShortcutReader.TryGetTarget(text));
					if (text2 == null)
					{
						continue;
					}
					text = text2;
				}
				Record(text, dateTime.Value);
			}
		}
	}

	private void ReadBam(IRegistryService registry)
	{
		string text = WindowsIdentity.GetCurrent().User?.Value;
		if (text == null)
		{
			return;
		}
		string key = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\bam\\State\\UserSettings\\" + text;
		Dictionary<string, string> devices = DeviceMap();
		foreach (string name in Safe(() => registry.GetValueNames(key)))
		{
			byte[] array = Safe(() => registry.ReadBinary(key, name));
			if (array == null || array.Length < 8)
			{
				continue;
			}
			string text2 = DevicePathToDosPath(name, devices);
			if (text2 != null)
			{
				DateTime? dateTime = FromFileTime(BitConverter.ToInt64(array, 0));
				if (dateTime.HasValue)
				{
					DateTime valueOrDefault = dateTime.GetValueOrDefault();
					Record(text2, valueOrDefault);
				}
			}
		}
	}

	private void ReadPrefetch()
	{
		string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
		try
		{
			foreach (FileInfo item in new DirectoryInfo(path).EnumerateFiles("*.pf"))
			{
				int num = item.Name.LastIndexOf('-');
				if (num > 0)
				{
					RecordName(item.Name.Substring(0, num), item.LastWriteTime);
				}
			}
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException) ? true : false)
		{
		}
	}

	private void ReadRunningProcesses()
	{
		DateTime now = DateTime.Now;
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			using (process)
			{
				try
				{
					string text = process.MainModule?.FileName;
					if (text != null)
					{
						Record(text, now);
					}
				}
				catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException || ex is NotSupportedException) ? true : false)
				{
				}
			}
		}
	}

	private void Record(string path, DateTime when)
	{
		if (!_byPath.TryGetValue(path, out var value) || when > value)
		{
			_byPath[path] = when;
		}
		RecordName(Path.GetFileName(path), when);
	}

	private void RecordName(string exe, DateTime when)
	{
		if (!string.IsNullOrEmpty(exe) && (!_byExeName.TryGetValue(exe, out var value) || when > value))
		{
			_byExeName[exe] = when;
		}
	}

	public static string Rot13(string value)
	{
		char[] array = value.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			char c = array[i];
			int num = i;
			char c2;
			switch (c)
			{
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				c2 = (char)(97 + (c - 97 + 13) % 26);
				break;
			case 'A':
			case 'B':
			case 'C':
			case 'D':
			case 'E':
			case 'F':
			case 'G':
			case 'H':
			case 'I':
			case 'J':
			case 'K':
			case 'L':
			case 'M':
			case 'N':
			case 'O':
			case 'P':
			case 'Q':
			case 'R':
			case 'S':
			case 'T':
			case 'U':
			case 'V':
			case 'W':
			case 'X':
			case 'Y':
			case 'Z':
				c2 = (char)(65 + (c - 65 + 13) % 26);
				break;
			default:
				c2 = c;
				break;
			}
			array[num] = c2;
		}
		return new string(array);
	}

	public static DateTime? ReadUserAssistTime(byte[]? data)
	{
		if (data == null || data.Length < 68)
		{
			return null;
		}
		return FromFileTime(BitConverter.ToInt64(data, 60));
	}

	private static DateTime? FromFileTime(long fileTime)
	{
		if (fileTime <= 0)
		{
			return null;
		}
		try
		{
			DateTime dateTime = DateTime.FromFileTime(fileTime);
			return (dateTime.Year >= 2000 && dateTime <= DateTime.Now.AddDays(1.0)) ? new DateTime?(dateTime) : ((DateTime?)null);
		}
		catch (ArgumentOutOfRangeException)
		{
			return null;
		}
	}

	private static string? ExpandKnownFolder(string value)
	{
		if (value.Length > 38 && value[0] == '{' && value[37] == '}' && Guid.TryParse(value.Substring(0, 38), out var result))
		{
			string text = KnownFolderPath(result);
			if (text != null)
			{
				return Path.Combine(text, value.Substring(39));
			}
			return null;
		}
		if (value.Length <= 2 || value[1] != ':')
		{
			return null;
		}
		return value;
	}

	private static string? KnownFolderPath(Guid id)
	{
		if (SHGetKnownFolderPath(ref id, 0u, IntPtr.Zero, out var path) != 0)
		{
			return null;
		}
		try
		{
			return Marshal.PtrToStringUni(path);
		}
		finally
		{
			Marshal.FreeCoTaskMem(path);
		}
	}

	private static Dictionary<string, string> DeviceMap()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string[] logicalDrives = Environment.GetLogicalDrives();
		for (int i = 0; i < logicalDrives.Length; i++)
		{
			string text = logicalDrives[i].TrimEnd('\\');
			StringBuilder stringBuilder = new StringBuilder(260);
			if (QueryDosDevice(text, stringBuilder, stringBuilder.Capacity) != 0)
			{
				dictionary[stringBuilder.ToString()] = text;
			}
		}
		return dictionary;
	}

	private static string? DevicePathToDosPath(string devicePath, Dictionary<string, string> devices)
	{
		foreach (var (text3, text4) in devices)
		{
			if (devicePath.StartsWith(text3 + "\\", StringComparison.OrdinalIgnoreCase))
			{
				return text4 + devicePath.Substring(text3.Length);
			}
		}
		return null;
	}

	private static bool IsSpecificFolder(string folder)
	{
		string[] source = new string[8]
		{
			Environment.GetFolderPath(Environment.SpecialFolder.Windows),
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
			Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
			Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
			Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
		};
		string trimmed = folder.TrimEnd('\\');
		if (trimmed.Length <= 3 || source.Any((string g) => trimmed.Equals(g, StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}
		string value = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + "\\";
		if (!folder.StartsWith(value, StringComparison.OrdinalIgnoreCase) && !folder.Contains("\\Package Cache\\", StringComparison.OrdinalIgnoreCase))
		{
			return !folder.Contains("\\InstallShield Installation Information\\", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsHousekeeping(string path)
	{
		string fileName = Path.GetFileName(path);
		if (!fileName.StartsWith("unins", StringComparison.OrdinalIgnoreCase) && !fileName.Contains("uninstall", StringComparison.OrdinalIgnoreCase) && !fileName.Contains("update", StringComparison.OrdinalIgnoreCase) && !fileName.Contains("crash", StringComparison.OrdinalIgnoreCase) && !fileName.Contains("setup", StringComparison.OrdinalIgnoreCase))
		{
			return fileName.Contains("installer", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static IReadOnlyList<string> Safe(Func<IReadOnlyList<string>> read)
	{
		try
		{
			return read();
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException) ? true : false)
		{
			return Array.Empty<string>();
		}
	}

	private static byte[]? Safe(Func<byte[]?> read)
	{
		try
		{
			return read();
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException) ? true : false)
		{
			return null;
		}
	}

	[DllImport("shell32.dll")]
	private static extern int SHGetKnownFolderPath(ref Guid rfid, uint flags, nint token, out nint path);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern uint QueryDosDevice(string deviceName, StringBuilder targetPath, int max);
}
