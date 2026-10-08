using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;

namespace GhostEye.Infrastructure.Cleanup;

public sealed class CleanupScanner(IAppLogger logger, ILocalizer loc) : ICleanupScanner
{
	private sealed record CategoryDefinition(string Id, string Name, string Description, IReadOnlyList<string> Roots, TimeSpan? MinimumAge = null, string? RunningProcess = null, string? FilePattern = null, IReadOnlyList<string>? StopServices = null, string? RunningName = null);

	private static readonly TimeSpan OldFileAge = TimeSpan.FromDays(30.0);

	public const string RecycleBinId = "recycle-bin";

	public static bool IsOptional(string categoryId)
	{
		if (!categoryId.StartsWith("cache-", StringComparison.Ordinal))
		{
			return categoryId == "recycle-bin";
		}
		return true;
	}

	public Task<CleanupResult> ScanAsync(IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			List<CleanupCategory> list = new List<CleanupCategory>();
			foreach (CategoryDefinition item in BuildDefinitions(loc))
			{
				ct.ThrowIfCancellationRequested();
				progress?.Report(item.Name);
				List<string> list2 = item.Roots.Where(Directory.Exists).ToList();
				if (list2.Count != 0)
				{
					var (bytes, num) = Measure(list2, item.MinimumAge, item.FilePattern, ct);
					if (num != 0)
					{
						list.Add(new CleanupCategory
						{
							Id = item.Id,
							Name = item.Name,
							Description = item.Description,
							Bytes = bytes,
							FileCount = num,
							Roots = list2,
							Warning = ((item.RunningProcess != null && IsRunning(item.RunningProcess)) ? loc.Format("cleanup.browserRunning", item.RunningName ?? item.RunningProcess) : null)
						});
					}
				}
			}
			progress?.Report(loc["cleanup.category.recycleBin.name"]);
			RecycleBin.Info? info = RecycleBin.Query();
			if (info.HasValue)
			{
				RecycleBin.Info valueOrDefault = info.GetValueOrDefault();
				if (valueOrDefault.Bytes > 0)
				{
					list.Add(new CleanupCategory
					{
						Id = "recycle-bin",
						Name = loc["cleanup.category.recycleBin.name"],
						Description = loc["cleanup.category.recycleBin.desc"],
						Bytes = valueOrDefault.Bytes,
						FileCount = (int)Math.Min(valueOrDefault.Items, 2147483647L),
						Roots = Array.Empty<string>()
					});
				}
			}
			return new CleanupResult
			{
				Categories = list
			};
		}, ct);
	}

	public Task<CleanupOutcome> CleanAsync(IEnumerable<CleanupCategory> categories, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			long num = 0L;
			int num2 = 0;
			int num3 = 0;
			foreach (CleanupCategory category in categories)
			{
				ct.ThrowIfCancellationRequested();
				progress?.Report(category.Name);
				if (category.Id == "recycle-bin")
				{
					if (RecycleBin.Empty())
					{
						num += category.Bytes;
						num2 += category.FileCount;
					}
					else
					{
						num3 += category.FileCount;
					}
				}
				else
				{
					CategoryDefinition categoryDefinition = BuildDefinitions(loc).FirstOrDefault((CategoryDefinition d) => d.Id == category.Id);
					List<string> names = StopServices(categoryDefinition?.StopServices);
					try
					{
						foreach (FileInfo item in EnumerateFiles(category.Roots, categoryDefinition?.MinimumAge, categoryDefinition?.FilePattern, ct))
						{
							try
							{
								long length = item.Length;
								item.Delete();
								num += length;
								num2++;
							}
							catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
							{
								num3++;
							}
						}
						if (categoryDefinition?.FilePattern == null)
						{
							RemoveEmptyDirectories(category.Roots, ct);
						}
					}
					finally
					{
						StartServices(names);
					}
				}
			}
			logger.Info($"Cleanup: {num2} files deleted, {num3} skipped, {num} bytes freed.");
			return new CleanupOutcome
			{
				FreedBytes = num,
				DeletedFiles = num2,
				SkippedFiles = num3
			};
		}, ct);
	}

	private static List<CategoryDefinition> BuildDefinitions(ILocalizer loc)
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
		string folderPath3 = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
		List<CategoryDefinition> list = new List<CategoryDefinition>();
		list.Add(new CategoryDefinition("temp-user", loc["cleanup.category.tempUser.name"], loc["cleanup.category.tempUser.desc"], new _003C_003Ez__ReadOnlySingleElementList<string>(Path.GetTempPath())));
		list.Add(new CategoryDefinition("temp-windows", loc["cleanup.category.tempWindows.name"], loc["cleanup.category.tempWindows.desc"], new _003C_003Ez__ReadOnlySingleElementList<string>(Path.Combine(folderPath2, "Temp"))));
		list.Add(new CategoryDefinition("crash-dump", loc["cleanup.category.crashDump.name"], loc["cleanup.category.crashDump.desc"], new _003C_003Ez__ReadOnlyArray<string>(new string[2]
		{
			Path.Combine(folderPath, "CrashDumps"),
			Path.Combine(folderPath2, "Minidump")
		})));
		list.Add(new CategoryDefinition("logs", loc["cleanup.category.logs.name"], loc["cleanup.category.logs.desc"], new _003C_003Ez__ReadOnlySingleElementList<string>(Path.Combine(folderPath2, "Logs")), OldFileAge));
		string name = loc["cleanup.category.windowsUpdate.name"];
		string description = loc["cleanup.category.windowsUpdate.desc"];
		_003C_003Ez__ReadOnlySingleElementList<string> roots = new _003C_003Ez__ReadOnlySingleElementList<string>(Path.Combine(folderPath2, "SoftwareDistribution\\Download"));
		IReadOnlyList<string> stopServices = new _003C_003Ez__ReadOnlyArray<string>(new string[2] { "wuauserv", "bits" });
		list.Add(new CategoryDefinition("windows-update", name, description, roots, null, null, null, stopServices));
		string name2 = loc["cleanup.category.deliveryOptimization.name"];
		string description2 = loc["cleanup.category.deliveryOptimization.desc"];
		_003C_003Ez__ReadOnlySingleElementList<string> roots2 = new _003C_003Ez__ReadOnlySingleElementList<string>(Path.Combine(folderPath2, "ServiceProfiles\\NetworkService\\AppData\\Local\\Microsoft\\Windows\\DeliveryOptimization\\Cache"));
		stopServices = new _003C_003Ez__ReadOnlySingleElementList<string>("DoSvc");
		list.Add(new CategoryDefinition("delivery-optimization", name2, description2, roots2, null, null, null, stopServices));
		list.Add(new CategoryDefinition("error-reports", loc["cleanup.category.errorReports.name"], loc["cleanup.category.errorReports.desc"], new _003C_003Ez__ReadOnlyArray<string>(new string[3]
		{
			Path.Combine(folderPath3, "Microsoft\\Windows\\WER\\ReportArchive"),
			Path.Combine(folderPath3, "Microsoft\\Windows\\WER\\ReportQueue"),
			Path.Combine(folderPath, "Microsoft\\Windows\\WER")
		})));
		list.Add(new CategoryDefinition("thumbnails", loc["cleanup.category.thumbnails.name"], loc["cleanup.category.thumbnails.desc"], new _003C_003Ez__ReadOnlySingleElementList<string>(Path.Combine(folderPath, "Microsoft\\Windows\\Explorer")), null, null, "thumbcache_*.db"));
		list.Add(new CategoryDefinition("cache-shaders", loc["cleanup.category.shaders.name"], loc["cleanup.category.shaders.desc"], new _003C_003Ez__ReadOnlyArray<string>(new string[5]
		{
			Path.Combine(folderPath, "D3DSCache"),
			Path.Combine(folderPath, "NVIDIA\\DXCache"),
			Path.Combine(folderPath, "NVIDIA\\GLCache"),
			Path.Combine(folderPath, "AMD\\DxCache"),
			Path.Combine(folderPath, "AMD\\GLCache")
		})));
		List<CategoryDefinition> list2 = list;
		foreach (CategoryDefinition item in BrowserCaches(folderPath, loc))
		{
			list2.Add(item);
		}
		return list2;
	}

	private static IEnumerable<CategoryDefinition> BrowserCaches(string local, ILocalizer loc)
	{
		(string, string, string, string[])[] array = new (string, string, string, string[])[4]
		{
			("cache-chrome", "Google Chrome", "chrome", new string[1] { Path.Combine(local, "Google\\Chrome\\User Data\\Default\\Cache") }),
			("cache-edge", "Microsoft Edge", "msedge", new string[1] { Path.Combine(local, "Microsoft\\Edge\\User Data\\Default\\Cache") }),
			("cache-brave", "Brave", "brave", new string[1] { Path.Combine(local, "BraveSoftware\\Brave-Browser\\User Data\\Default\\Cache") }),
			("cache-firefox", "Firefox", "firefox", new string[1] { Path.Combine(local, "Mozilla\\Firefox\\Profiles") })
		};
		(string Id, string Name, string Process, string[] Paths)[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			(string, string, string, string[]) tuple = array2[i];
			if (tuple.Item4.Any(Directory.Exists))
			{
				string item = tuple.Item1;
				string name = loc.Format("cleanup.category.browserCache.name", tuple.Item2);
				string description = loc["cleanup.category.browserCache.desc"];
				string[] item2 = tuple.Item4;
				string item3 = tuple.Item3;
				string item4 = tuple.Item2;
				yield return new CategoryDefinition(item, name, description, item2, null, item3, null, null, item4);
			}
		}
	}

	private static bool IsRunning(string processName)
	{
		try
		{
			return Process.GetProcessesByName(processName).Length != 0;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}

	private List<string> StopServices(IReadOnlyList<string>? names)
	{
		List<string> list = new List<string>();
		foreach (string item in names ?? Array.Empty<string>())
		{
			try
			{
				using ServiceController serviceController = new ServiceController(item);
				if (serviceController.Status != ServiceControllerStatus.Stopped)
				{
					serviceController.Stop();
					serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30.0));
					list.Add(item);
				}
			}
			catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception || ex is System.ServiceProcess.TimeoutException) ? true : false)
			{
				logger.Info("Cleanup: could not stop service " + item + ": " + ex.Message);
			}
		}
		return list;
	}

	private void StartServices(IEnumerable<string> names)
	{
		foreach (string name in names)
		{
			try
			{
				using ServiceController serviceController = new ServiceController(name);
				serviceController.Start();
			}
			catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception) ? true : false)
			{
				logger.Info("Cleanup: could not restart service " + name + ": " + ex.Message);
			}
		}
	}

	private static (long Bytes, int Count) Measure(IEnumerable<string> roots, TimeSpan? minimumAge, string? pattern, CancellationToken ct)
	{
		long num = 0L;
		int num2 = 0;
		foreach (FileInfo item in EnumerateFiles(roots, minimumAge, pattern, ct))
		{
			num += item.Length;
			num2++;
		}
		return (Bytes: num, Count: num2);
	}

	private static IEnumerable<FileInfo> EnumerateFiles(IEnumerable<string> roots, TimeSpan? minimumAge, string? pattern, CancellationToken ct)
	{
		DateTime? cutoff = ((!minimumAge.HasValue) ? ((DateTime?)null) : new DateTime?(DateTime.Now - minimumAge.Value));
		Stack<string> pending = new Stack<string>(roots);
		while (pending.Count > 0)
		{
			ct.ThrowIfCancellationRequested();
			string current = pending.Pop();
			FileInfo[] files;
			try
			{
				files = new DirectoryInfo(current).GetFiles(pattern ?? "*");
			}
			catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is DirectoryNotFoundException || ex is IOException) ? true : false)
			{
				continue;
			}
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				if (!cutoff.HasValue || !(fileInfo.LastWriteTime > cutoff.Value))
				{
					yield return fileInfo;
				}
			}
			if (pattern != null)
			{
				continue;
			}
			try
			{
				string[] directories = Directory.GetDirectories(current);
				foreach (string item in directories)
				{
					pending.Push(item);
				}
			}
			catch (Exception ex2) when ((ex2 is UnauthorizedAccessException || ex2 is DirectoryNotFoundException || ex2 is IOException) ? true : false)
			{
			}
		}
	}

	private static void RemoveEmptyDirectories(IEnumerable<string> roots, CancellationToken ct)
	{
		foreach (string root in roots)
		{
			if (!Directory.Exists(root))
			{
				continue;
			}
			string[] directories;
			try
			{
				directories = Directory.GetDirectories(root, "*", SearchOption.AllDirectories);
			}
			catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is IOException) ? true : false)
			{
				continue;
			}
			foreach (string item in directories.OrderByDescending((string d) => d.Length))
			{
				ct.ThrowIfCancellationRequested();
				try
				{
					if (Directory.GetFileSystemEntries(item).Length == 0)
					{
						Directory.Delete(item);
					}
				}
				catch (Exception ex2) when ((ex2 is UnauthorizedAccessException || ex2 is IOException) ? true : false)
				{
				}
			}
		}
	}
}
