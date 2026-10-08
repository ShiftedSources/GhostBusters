using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;

namespace GhostEye.Infrastructure.Platform;

public sealed class PerformanceToolsService(IProcessRunner processRunner, ILocalizer loc) : IPerformanceToolsService
{
	private struct MemoryStatusEx
	{
		public uint dwLength;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;
	}

	private struct Luid
	{
		public uint LowPart;

		public int HighPart;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct TokenPrivileges
	{
		public uint PrivilegeCount;

		public Luid Luid;

		public uint Attributes;
	}

	private static readonly HashSet<string> Protected = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"System", "Idle", "Registry", "smss", "csrss", "wininit", "winlogon", "services", "lsass", "svchost",
		"dwm", "explorer", "fontdrvhost", "Memory Compression", "MsMpEng", "audiodg", "sihost", "ctfmon", "SearchHost", "StartMenuExperienceHost",
		"ShellExperienceHost", "RuntimeBroker", "conhost", "spoolsv", "WmiPrvSE", "NisSrv", "SecurityHealthService", "GhostEye"
	};

	public MemoryStatus GetMemoryStatus()
	{
		MemoryStatusEx buffer = new MemoryStatusEx
		{
			dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>()
		};
		if (!GlobalMemoryStatusEx(ref buffer))
		{
			return default;
		}
		return new MemoryStatus((long)buffer.ullTotalPhys, (long)buffer.ullAvailPhys);
	}

	public Task<MemoryCleanResult> FreeMemoryAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			long availableBytes = GetMemoryStatus().AvailableBytes;
			int num = 0;
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				ct.ThrowIfCancellationRequested();
				using (process)
				{
					if (process.Id > 4 && !Protected.Contains(process.ProcessName))
					{
						try
						{
							if (EmptyWorkingSet(process.Handle))
							{
								num++;
							}
						}
						catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException || ex is NotSupportedException) ? true : false)
						{
						}
					}
				}
			}
			bool flag = PurgeStandbyList();
			Thread.Sleep(500);
			long freedBytes = Math.Max(0L, GetMemoryStatus().AvailableBytes - availableBytes);
			string message = (flag ? loc.Format("tools.ram.result", num) : loc.Format("tools.ram.resultNoStandby", num));
			return new MemoryCleanResult(Success: true, freedBytes, message);
		}, ct);
	}

	public async Task<IReadOnlyList<ProcessUsage>> GetHeavyProcessesAsync(int count, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<int, TimeSpan> first = SampleCpu();
		Stopwatch started = Stopwatch.StartNew();
		await Task.Delay(1000, ct).ConfigureAwait(continueOnCapturedContext: false);
		Dictionary<int, TimeSpan> second = SampleCpu();
		double elapsed = started.Elapsed.TotalMilliseconds * (double)Environment.ProcessorCount;
		return await Task.Run(() =>
		{
			List<ProcessUsage> list = new List<ProcessUsage>();
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				using (process)
				{
					if (process.Id > 4 && !Protected.Contains(process.ProcessName))
					{
						try
						{
							double value = ((first.TryGetValue(process.Id, out var value2) && second.TryGetValue(process.Id, out var value3)) ? Math.Max(0.0, (value3 - value2).TotalMilliseconds / elapsed * 100.0) : 0.0);
							string path = null;
							try
							{
								path = process.MainModule?.FileName;
							}
							catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
							{
							}
							list.Add(new ProcessUsage(process.Id, process.ProcessName, path, process.WorkingSet64, Math.Round(value, 1)));
						}
						catch (InvalidOperationException)
						{
						}
					}
				}
			}
			return (from p in list.GroupBy((ProcessUsage p) => p.Name, StringComparer.OrdinalIgnoreCase).Select((IGrouping<string, ProcessUsage> g) =>
				{
					Func<ProcessUsage, long> keySelector = (ProcessUsage p) => p.WorkingSetBytes;
					return g.OrderByDescending(keySelector).First()with
					{
						WorkingSetBytes = g.Sum((ProcessUsage p) => p.WorkingSetBytes),
						CpuPercent = Math.Round(g.Sum((ProcessUsage p) => p.CpuPercent), 1)
					};
				})
				orderby p.WorkingSetBytes descending
				select p).Take(count).ToList();
		}, ct).ConfigureAwait(continueOnCapturedContext: false);
	}

	public OperationResult EndProcess(int processId)
	{
		try
		{
			using Process process = Process.GetProcessById(processId);
			if (Protected.Contains(process.ProcessName))
			{
				return OperationResult.Fail(loc["tools.processes.protected"]);
			}
			Process[] processesByName = Process.GetProcessesByName(process.ProcessName);
			foreach (Process process2 in processesByName)
			{
				using (process2)
				{
					try
					{
						process2.Kill(entireProcessTree: true);
					}
					catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
					{
					}
				}
			}
			return OperationResult.Ok;
		}
		catch (ArgumentException)
		{
			return OperationResult.Ok;
		}
		catch (Exception ex3) when ((ex3 is Win32Exception || ex3 is InvalidOperationException) ? true : false)
		{
			return OperationResult.Fail(ex3.Message);
		}
	}

	public async Task<OperationResult> FlushDnsAsync(CancellationToken ct = default(CancellationToken))
	{
		ProcessResult processResult = await processRunner.RunAsync("ipconfig.exe", "/flushdns", ct).ConfigureAwait(continueOnCapturedContext: false);
		return processResult.Success ? OperationResult.Ok : OperationResult.Fail(processResult.Combined.Trim());
	}

	public Task<IReadOnlyList<LargeFile>> FindLargeFilesAsync(long minimumBytes, int maxResults, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		return Task.Run((Func<IReadOnlyList<LargeFile>>)(() =>
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			List<LargeFile> list = new List<LargeFile>();
			Stack<string> stack = new Stack<string>();
			stack.Push(folderPath);
			while (stack.Count > 0)
			{
				ct.ThrowIfCancellationRequested();
				string text = stack.Pop();
				if (!text.Equals(Path.Combine(folderPath, "AppData"), StringComparison.OrdinalIgnoreCase))
				{
					progress?.Report(text);
					try
					{
						DirectoryInfo directoryInfo = new DirectoryInfo(text);
						if (!directoryInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) || !(text != folderPath))
						{
							foreach (FileInfo item in directoryInfo.EnumerateFiles())
							{
								if (item.Length >= minimumBytes && !item.Attributes.HasFlag(FileAttributes.System))
								{
									list.Add(new LargeFile(item.FullName, item.Length, item.LastWriteTime));
								}
							}
							foreach (DirectoryInfo item2 in directoryInfo.EnumerateDirectories())
							{
								stack.Push(item2.FullName);
							}
						}
					}
					catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException) ? true : false)
					{
					}
				}
			}
			return list.OrderByDescending((LargeFile f) => f.Bytes).Take(maxResults).ToList();
		}), ct);
	}

	private static Dictionary<int, TimeSpan> SampleCpu()
	{
		Dictionary<int, TimeSpan> dictionary = new Dictionary<int, TimeSpan>();
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			using (process)
			{
				try
				{
					dictionary[process.Id] = process.TotalProcessorTime;
				}
				catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException || ex is NotSupportedException) ? true : false)
				{
				}
			}
		}
		return dictionary;
	}

	private static bool PurgeStandbyList()
	{
		if (!EnablePrivilege("SeProfileSingleProcessPrivilege"))
		{
			return false;
		}
		GCHandle gCHandle = GCHandle.Alloc(4, GCHandleType.Pinned);
		try
		{
			return NtSetSystemInformation(80, gCHandle.AddrOfPinnedObject(), 4) == 0;
		}
		finally
		{
			gCHandle.Free();
		}
	}

	private static bool EnablePrivilege(string privilege)
	{
		if (!OpenProcessToken(Process.GetCurrentProcess().Handle, 40u, out var tokenHandle))
		{
			return false;
		}
		try
		{
			if (!LookupPrivilegeValue(null, privilege, out var luid))
			{
				return false;
			}
			TokenPrivileges newState = new TokenPrivileges
			{
				PrivilegeCount = 1u,
				Luid = luid,
				Attributes = 2u
			};
			return AdjustTokenPrivileges(tokenHandle, disableAll: false, ref newState, 0u, IntPtr.Zero, IntPtr.Zero) && Marshal.GetLastWin32Error() == 0;
		}
		finally
		{
			CloseHandle(tokenHandle);
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

	[DllImport("psapi.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool EmptyWorkingSet(nint process);

	[DllImport("ntdll.dll")]
	private static extern int NtSetSystemInformation(int infoClass, nint info, int length);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool OpenProcessToken(nint processHandle, uint desiredAccess, out nint tokenHandle);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool LookupPrivilegeValue(string? systemName, string name, out Luid luid);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AdjustTokenPrivileges(nint tokenHandle, bool disableAll, ref TokenPrivileges newState, uint bufferLength, nint previousState, nint returnLength);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(nint handle);
}
