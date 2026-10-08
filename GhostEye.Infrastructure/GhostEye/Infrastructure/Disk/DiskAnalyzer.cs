using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Disk;

public sealed class DiskAnalyzer : IDiskAnalyzer
{
	private const int PartialHashBytes = 65536;

	private readonly ConcurrentDictionary<string, (long Bytes, long Files)> _cache = new ConcurrentDictionary<string, (long, long)>(StringComparer.OrdinalIgnoreCase);

	private const int RecallOnOpen = 262144;

	private const int RecallOnDataAccess = 4194304;

	public IReadOnlyList<DriveSummary> GetDrives()
	{
		return (from d in DriveInfo.GetDrives()
			where d.IsReady && d.DriveType == DriveType.Fixed
			select new DriveSummary(d.RootDirectory.FullName, string.IsNullOrWhiteSpace(d.VolumeLabel) ? d.Name.TrimEnd('\\') : (d.VolumeLabel + " (" + d.Name.TrimEnd('\\') + ")"), d.TotalSize, d.TotalFreeSpace)).ToList();
	}

	public Task<IReadOnlyList<FolderUsage>> MeasureAsync(string folder, IProgress<string>? progress, CancellationToken ct = default(CancellationToken))
	{
		return Task.Run((Func<IReadOnlyList<FolderUsage>>)(() =>
		{
			ConcurrentBag<FolderUsage> result = new ConcurrentBag<FolderUsage>();
			DirectoryInfo directoryInfo = new DirectoryInfo(folder);
			long num = 0L;
			long num2 = 0L;
			try
			{
				foreach (FileInfo item in directoryInfo.EnumerateFiles())
				{
					if (!IsCloudPlaceholder(item.Attributes))
					{
						num += item.Length;
					}
					num2++;
				}
			}
			catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException) ? true : false)
			{
			}
			if (num2 > 0)
			{
				result.Add(new FolderUsage(folder, string.Empty, num, num2, IsFilesEntry: true, IsInaccessible: false));
			}
			List<DirectoryInfo> source;
			try
			{
				source = (from d in directoryInfo.EnumerateDirectories()
					where !d.Attributes.HasFlag(FileAttributes.ReparsePoint)
					select d).ToList();
			}
			catch (Exception ex2) when ((ex2 is UnauthorizedAccessException || ex2 is IOException || ex2 is SecurityException) ? true : false)
			{
				source = new List<DirectoryInfo>();
			}
			Parallel.ForEach(source, new ParallelOptions
			{
				MaxDegreeOfParallelism = Math.Max(2, Environment.ProcessorCount / 2),
				CancellationToken = ct
			}, (DirectoryInfo child) =>
			{
				progress?.Report(child.FullName);
				var (num3, files, flag) = Size(child.FullName, ct);
				result.Add(new FolderUsage(child.FullName, child.Name, num3, files, IsFilesEntry: false, flag && num3 == 0));
			});
			return result.OrderByDescending((FolderUsage r) => r.Bytes).ToList();
		}), ct);
	}

	private (long Bytes, long Files, bool Denied) Size(string root, CancellationToken ct)
	{
		if (_cache.TryGetValue(root, out (long, long) value))
		{
			return (Bytes: value.Item1, Files: value.Item2, Denied: false);
		}
		List<(string, int, long, long, bool)> list = new List<(string, int, long, long, bool)>();
		bool item = false;
		Stack<(string, int)> stack = new Stack<(string, int)>();
		stack.Push((root, -1));
		while (stack.Count > 0)
		{
			ct.ThrowIfCancellationRequested();
			var (text, item2) = stack.Pop();
			if (_cache.TryGetValue(text, out (long, long) value2))
			{
				list.Add((text, item2, value2.Item1, value2.Item2, true));
				continue;
			}
			long num = 0L;
			long num2 = 0L;
			DirectoryInfo[] directories;
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(text);
				FileInfo[] files = directoryInfo.GetFiles();
				foreach (FileInfo fileInfo in files)
				{
					if (!IsCloudPlaceholder(fileInfo.Attributes))
					{
						num += fileInfo.Length;
					}
					num2++;
				}
				directories = directoryInfo.GetDirectories();
			}
			catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException) ? true : false)
			{
				item = true;
				list.Add((text, item2, 0L, 0L, true));
				continue;
			}
			int count = list.Count;
			list.Add((text, item2, num, num2, false));
			DirectoryInfo[] array = directories;
			foreach (DirectoryInfo directoryInfo2 in array)
			{
				if (!directoryInfo2.Attributes.HasFlag(FileAttributes.ReparsePoint))
				{
					stack.Push((directoryInfo2.FullName, count));
				}
			}
		}
		(long, long)[] array2 = list.Select(((string Path, int Parent, long Bytes, long Files, bool Cached) n) => (Bytes: n.Bytes, Files: n.Files)).ToArray();
		for (int num3 = list.Count - 1; num3 >= 0; num3--)
		{
			(string, int, long, long, bool) tuple2 = list[num3];
			if (!tuple2.Item5)
			{
				_cache[tuple2.Item1] = array2[num3];
			}
			if (tuple2.Item2 >= 0)
			{
				array2[tuple2.Item2] = (array2[tuple2.Item2].Item1 + array2[num3].Item1, array2[tuple2.Item2].Item2 + array2[num3].Item2);
			}
		}
		return (Bytes: array2[0].Item1, Files: array2[0].Item2, Denied: item);
	}

	private static bool IsCloudPlaceholder(FileAttributes attributes)
	{
		if (!attributes.HasFlag(FileAttributes.Offline))
		{
			return (attributes & (FileAttributes)0x440000) != 0;
		}
		return true;
	}

	public Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(IReadOnlyList<string> roots, long minimumBytes, IProgress<string>? progress, CancellationToken ct = default(CancellationToken))
	{
		return Task.Run((Func<IReadOnlyList<DuplicateGroup>>)(() =>
		{
			Dictionary<long, List<FileInfo>> dictionary = new Dictionary<long, List<FileInfo>>();
			foreach (FileInfo item in EnumerateFiles(roots, progress, ct))
			{
				if (item.Length >= minimumBytes)
				{
					if (!dictionary.TryGetValue(item.Length, out var value))
					{
						value = new List<FileInfo>();
						dictionary[item.Length] = value;
					}
					value.Add(item);
				}
			}
			List<DuplicateGroup> list = new List<DuplicateGroup>();
			foreach (var (sizeBytes, source) in dictionary.Where((KeyValuePair<long, List<FileInfo>> p) => p.Value.Count > 1))
			{
				ct.ThrowIfCancellationRequested();
				foreach (IGrouping<string, FileInfo> item2 in from f in source
					group f by TryHash(f.FullName, partialOnly: true) into g
					where g.Key != null && g.Count() > 1
					select g)
				{
					progress?.Report(item2.First().FullName);
					foreach (IGrouping<string, FileInfo> item3 in from f in item2
						group f by TryHash(f.FullName, partialOnly: false) into g
						where g.Key != null && g.Count() > 1
						select g)
					{
						list.Add(new DuplicateGroup(item3.Key, sizeBytes, (from f in item3
							select new DuplicateFile(f.FullName, f.LastWriteTime) into f
							orderby f.LastWrite descending
							select f).ToList()));
					}
				}
			}
			return list.OrderByDescending((DuplicateGroup g) => g.WastedBytes).ToList();
		}), ct);
	}

	private static IEnumerable<FileInfo> EnumerateFiles(IEnumerable<string> roots, IProgress<string>? progress, CancellationToken ct)
	{
		Stack<string> pending = new Stack<string>(roots.Where(Directory.Exists));
		string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData");
		while (pending.Count > 0)
		{
			ct.ThrowIfCancellationRequested();
			string text = pending.Pop();
			if (text.Equals(appData, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			progress?.Report(text);
			FileInfo[] files;
			DirectoryInfo[] subs;
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(text);
				files = directoryInfo.GetFiles();
				subs = directoryInfo.GetDirectories();
			}
			catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is IOException || ex is SecurityException) ? true : false)
			{
				continue;
			}
			FileInfo[] array = files;
			foreach (FileInfo fileInfo in array)
			{
				if (!fileInfo.Attributes.HasFlag(FileAttributes.System) && !fileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) && !IsCloudPlaceholder(fileInfo.Attributes))
				{
					yield return fileInfo;
				}
			}
			DirectoryInfo[] array2 = subs;
			foreach (DirectoryInfo directoryInfo2 in array2)
			{
				if (!directoryInfo2.Attributes.HasFlag(FileAttributes.ReparsePoint) && !directoryInfo2.Name.StartsWith('.'))
				{
					pending.Push(directoryInfo2.FullName);
				}
			}
		}
	}

	public static string? TryHash(string path, bool partialOnly)
	{
		try
		{
			using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 65536, FileOptions.SequentialScan);
			if (!partialOnly || fileStream.Length <= 131072)
			{
				return Convert.ToHexString(SHA256.HashData(fileStream));
			}
			byte[] array = new byte[131072];
			fileStream.ReadExactly(array, 0, 65536);
			fileStream.Seek(-65536L, SeekOrigin.End);
			fileStream.ReadExactly(array, 65536, 65536);
			return Convert.ToHexString(SHA256.HashData(array));
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			return null;
		}
	}
}
