using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GhostEye.Infrastructure.Games;

public static class RunningGames
{
	private const uint ProcessQueryLimitedInformation = 4096u;

	private static readonly string[] HelperFragments = new string[15]
	{
		"launcher", "crash", "report", "unins", "setup", "redist", "dxsetup", "vcredist", "easyanticheat", "battleye",
		"beservice", "updater", "helper", "webhelper", "overlay"
	};

	public static RunningGame? FindFirst(IReadOnlyList<GameEntry> library, IReadOnlyCollection<string>? excludedIds = null)
	{
		if (library.Count == 0)
		{
			return null;
		}
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			using (process)
			{
				if (process.Id <= 4)
				{
					continue;
				}
				string path = GetPath(process.Id);
				if (path == null || IsHelper(path))
				{
					continue;
				}
				GameEntry gameEntry = library.FirstOrDefault((GameEntry g) => g.Owns(path));
				if ((object)gameEntry == null || (excludedIds != null && excludedIds.Contains(gameEntry.Id, StringComparer.OrdinalIgnoreCase)))
				{
					continue;
				}
				try
				{
					if (process.MainWindowHandle == IntPtr.Zero)
					{
						continue;
					}
				}
				catch (InvalidOperationException)
				{
					goto end_IL_001d;
				}
				return new RunningGame(gameEntry, process.Id, path);
				end_IL_001d:;
			}
		}
		return null;
	}

	public static bool IsRunning(int processId)
	{
		try
		{
			using Process process = Process.GetProcessById(processId);
			return !process.HasExited;
		}
		catch (Exception ex) when ((ex is ArgumentException || ex is InvalidOperationException || ex is Win32Exception) ? true : false)
		{
			return false;
		}
	}

	internal static bool IsHelper(string path)
	{
		string name = Path.GetFileNameWithoutExtension(path);
		return HelperFragments.Any((string f) => name.Contains(f, StringComparison.OrdinalIgnoreCase));
	}

	public static string? GetPath(int processId)
	{
		nint num = OpenProcess(4096u, inheritHandle: false, processId);
		if (num == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			int size = stringBuilder.Capacity;
			return QueryFullProcessImageName(num, 0, stringBuilder, ref size) ? stringBuilder.ToString(0, size) : null;
		}
		finally
		{
			CloseHandle(num);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern nint OpenProcess(uint access, bool inheritHandle, int processId);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool QueryFullProcessImageName(nint process, int flags, StringBuilder name, ref int size);

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(nint handle);
}
