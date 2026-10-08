using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Platform;

public sealed class StartupManagerService : IStartupManagerService
{
	private const string RunUser = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	private const string RunMachine = "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	private const string RunMachine32 = "HKLM32\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	private const string ApprovedUser = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved";

	private const string ApprovedMachine = "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved";

	private const string BackupRoot = "HKCU\\Software\\GhostEye\\StartupBackup";

	private readonly IRegistryService _registry;

	private readonly IProcessRunner _processRunner;

	private readonly ILocalizer _loc;

	private readonly string _folderBackupRoot;

	private static string UserStartupFolder => Environment.GetFolderPath(Environment.SpecialFolder.Startup);

	private static string CommonStartupFolder => Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);

	public StartupManagerService(IRegistryService registry, IProcessRunner processRunner, ILocalizer loc, string folderBackupRoot)
	{
		_registry = registry;
		_processRunner = processRunner;
		_loc = loc;
		_folderBackupRoot = folderBackupRoot;
	}

	public IReadOnlyList<StartupEntry> GetEntries()
	{
		List<StartupEntry> list = new List<StartupEntry>();
		AddRunEntries(list, StartupSource.RunUser);
		AddRunEntries(list, StartupSource.RunMachine);
		AddRunEntries(list, StartupSource.RunMachine32);
		AddFolderEntries(list, StartupSource.FolderUser);
		AddFolderEntries(list, StartupSource.FolderCommon);
		AddRemovedEntries(list);
		return list;
	}

	public async Task<IReadOnlyList<StartupEntry>> GetAllEntriesAsync(CancellationToken ct = default(CancellationToken))
	{
		List<StartupEntry> entries = new List<StartupEntry>(GetEntries());
		List<StartupEntry> list = entries;
		list.AddRange(await GetScheduledTasksAsync(ct).ConfigureAwait(continueOnCapturedContext: false));
		return entries;
	}

	private void AddRunEntries(List<StartupEntry> entries, StartupSource source)
	{
		string text = RunKeyFor(source);
		foreach (string item in SafeValueNames(text))
		{
			if (!string.IsNullOrEmpty(item))
			{
				string command = _registry.ReadString(text, item) ?? string.Empty;
				entries.Add(new StartupEntry(item, command, source, IsApprovedEnabled(source, item), DisplayKey(text)));
			}
		}
	}

	private void AddFolderEntries(List<StartupEntry> entries, StartupSource source)
	{
		string text = ((source == StartupSource.FolderUser) ? UserStartupFolder : CommonStartupFolder);
		if (string.IsNullOrEmpty(text) || !Directory.Exists(text))
		{
			return;
		}
		IEnumerable<string> enumerable;
		try
		{
			enumerable = Directory.EnumerateFiles(text).ToList();
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			return;
		}
		foreach (string item in enumerable)
		{
			string fileName = Path.GetFileName(item);
			if (!fileName.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase))
			{
				string command = (Path.GetExtension(item).Equals(".lnk", StringComparison.OrdinalIgnoreCase) ? (ShortcutReader.TryGetTarget(item) ?? item) : item);
				entries.Add(new StartupEntry(fileName, command, source, IsApprovedEnabled(source, fileName), text));
			}
		}
	}

	private void AddRemovedEntries(List<StartupEntry> entries)
	{
		StartupScope[] values = Enum.GetValues<StartupScope>();
		foreach (StartupScope scope in values)
		{
			string text = BackupKeyFor(scope);
			if (!_registry.KeyExists(text))
			{
				continue;
			}
			foreach (string item in SafeValueNames(text))
			{
				string command = _registry.ReadString(text, item) ?? string.Empty;
				entries.Add(new StartupEntry(item, command, RemovedSourceFor(scope), IsEnabled: false, DisplayKey(text)));
			}
		}
		StartupSource[] array = new StartupSource[2]
		{
			StartupSource.RemovedFolderUser,
			StartupSource.RemovedFolderCommon
		};
		foreach (StartupSource startupSource in array)
		{
			string text2 = FolderBackupFor(startupSource);
			if (!Directory.Exists(text2))
			{
				continue;
			}
			try
			{
				foreach (string item2 in Directory.EnumerateFiles(text2).ToList())
				{
					string command2 = ShortcutReader.TryGetTarget(item2) ?? item2;
					entries.Add(new StartupEntry(Path.GetFileName(item2), command2, startupSource, IsEnabled: false, text2));
				}
			}
			catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
			{
			}
		}
	}

	private async Task<IReadOnlyList<StartupEntry>> GetScheduledTasksAsync(CancellationToken ct)
	{
		ProcessResult processResult;
		try
		{
			processResult = await _processRunner.RunPowerShellAsync("[Console]::OutputEncoding=[Text.Encoding]::UTF8;$t = Get-ScheduledTask | Where-Object { $_.TaskPath -notlike '\\Microsoft\\*' -and ($_.Triggers | Where-Object { $_.CimClass.CimClassName -in @('MSFT_TaskLogonTrigger','MSFT_TaskBootTrigger') }) } | ForEach-Object { [pscustomobject]@{ P = $_.TaskPath; N = $_.TaskName; S = [string]$_.State; E = (($_.Actions | Where-Object { $_.Execute } | ForEach-Object { ($_.Execute + ' ' + $_.Arguments).Trim() }) -join '; ') } };if ($t) { @($t) | ConvertTo-Json -Compress } else { '[]' }", ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
		{
			return Array.Empty<StartupEntry>();
		}
		if (!processResult.Success || string.IsNullOrWhiteSpace(processResult.StandardOutput))
		{
			return Array.Empty<StartupEntry>();
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(processResult.StandardOutput.Trim());
			List<JsonElement> obj = ((jsonDocument.RootElement.ValueKind == JsonValueKind.Array) ? jsonDocument.RootElement.EnumerateArray().ToList() : new List<JsonElement> { jsonDocument.RootElement });
			List<StartupEntry> list = new List<StartupEntry>();
			foreach (JsonElement item in obj)
			{
				string text = item.GetProperty("P").GetString() ?? "\\";
				string text2 = item.GetProperty("N").GetString() ?? string.Empty;
				string text3 = item.GetProperty("S").GetString() ?? string.Empty;
				string command = (item.TryGetProperty("E", out var value) ? (value.GetString() ?? string.Empty) : string.Empty);
				list.Add(new StartupEntry(text2, command, StartupSource.ScheduledTask, !text3.Equals("Disabled", StringComparison.OrdinalIgnoreCase), text, text + text2));
			}
			return list;
		}
		catch (Exception ex2) when ((ex2 is JsonException || ex2 is KeyNotFoundException || ex2 is InvalidOperationException) ? true : false)
		{
			return Array.Empty<StartupEntry>();
		}
	}

	public async Task<StartupOperationResult> SetEnabledAsync(StartupEntry entry, bool enabled, CancellationToken ct = default(CancellationToken))
	{
		ArgumentNullException.ThrowIfNull(entry, "entry");
		if (entry.IsRemoved)
		{
			return enabled ? Restore(entry) : StartupOperationResult.Ok;
		}
		if (entry.Source == StartupSource.ScheduledTask)
		{
			return await SetTaskEnabledAsync(entry, enabled, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		string approvedKey = ApprovedKeyFor(entry.Source);
		if (!RegistryService.TryRun(_loc, () =>
		{
			_registry.WriteValue(approvedKey, entry.Name, BuildApprovedValue(enabled), RegistryValueKind.Binary);
		}, out string error))
		{
			return StartupOperationResult.Fail(error);
		}
		return (IsApprovedEnabled(entry.Source, entry.Name) == enabled) ? StartupOperationResult.Ok : StartupOperationResult.Fail(_loc["startup.error.notApplied"]);
	}

	private async Task<StartupOperationResult> SetTaskEnabledAsync(StartupEntry entry, bool enabled, CancellationToken ct)
	{
		if (string.IsNullOrEmpty(entry.TaskPath))
		{
			return StartupOperationResult.Fail(_loc["startup.error.notApplied"]);
		}
		ProcessResult processResult = await _processRunner.RunAsync("schtasks.exe", "/Change /TN \"" + entry.TaskPath + "\" " + (enabled ? "/ENABLE" : "/DISABLE"), ct).ConfigureAwait(continueOnCapturedContext: false);
		return processResult.Success ? StartupOperationResult.Ok : StartupOperationResult.Fail(FirstLine(processResult.Combined) ?? _loc["startup.error.notApplied"]);
	}

	public static byte[] BuildApprovedValue(bool enabled)
	{
		byte[] array = new byte[12]
		{
			(byte)(enabled ? 2 : 3),
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0
		};
		if (!enabled)
		{
			BitConverter.GetBytes(DateTime.UtcNow.ToFileTimeUtc()).CopyTo(array, 4);
		}
		return array;
	}

	public static bool IsEnabledFlag(byte[]? value)
	{
		if (value != null && value.Length != 0)
		{
			return (value[0] & 1) == 0;
		}
		return true;
	}

	private bool IsApprovedEnabled(StartupSource source, string name)
	{
		try
		{
			return IsEnabledFlag(_registry.ReadBinary(ApprovedKeyFor(source), name));
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException) ? true : false)
		{
			return true;
		}
	}

	public StartupOperationResult Remove(StartupEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry, "entry");
		switch (entry.Source)
		{
		case StartupSource.RunUser:
		case StartupSource.RunMachine:
		case StartupSource.RunMachine32:
		{
			string runKey = RunKeyFor(entry.Source);
			string backupKey = BackupKeyFor(ScopeFor(entry.Source));
			if (_registry.ReadValue(runKey, entry.Name) == null)
			{
				return StartupOperationResult.Fail(_loc["startup.error.missing"]);
			}
			if (!RegistryService.TryRun(_loc, () =>
			{
				object value = _registry.ReadValue(runKey, entry.Name);
				RegistryValueKind kind = _registry.GetValueKind(runKey, entry.Name) ?? RegistryValueKind.String;
				_registry.WriteValue(backupKey, entry.Name, value, kind);
				_registry.DeleteValue(runKey, entry.Name);
				_registry.DeleteValue(ApprovedKeyFor(entry.Source), entry.Name);
			}, out string error2))
			{
				return StartupOperationResult.Fail(error2);
			}
			if (_registry.ReadValue(runKey, entry.Name) != null)
			{
				return StartupOperationResult.Fail(_loc["startup.error.notApplied"]);
			}
			return StartupOperationResult.Ok;
		}
		case StartupSource.FolderUser:
		case StartupSource.FolderCommon:
		{
			string sourceFileName = Path.Combine(entry.Location, entry.Name);
			string text = FolderBackupFor((entry.Source == StartupSource.FolderUser) ? StartupSource.RemovedFolderUser : StartupSource.RemovedFolderCommon);
			try
			{
				Directory.CreateDirectory(text);
				File.Move(sourceFileName, Path.Combine(text, entry.Name), overwrite: true);
				RegistryService.TryRun(_loc, () =>
				{
					_registry.DeleteValue(ApprovedKeyFor(entry.Source), entry.Name);
				}, out string _);
				return StartupOperationResult.Ok;
			}
			catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
			{
				return StartupOperationResult.Fail(ex.Message);
			}
		}
		default:
			return StartupOperationResult.Fail(_loc["startup.error.cannotRemove"]);
		}
	}

	public StartupOperationResult Restore(StartupEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry, "entry");
		switch (entry.Source)
		{
		case StartupSource.RemovedUser:
		case StartupSource.RemovedMachine:
		case StartupSource.RemovedMachine32:
		{
			StartupSource source = entry.Source;
			if (!TryRestore(entry.Name, source switch
			{
				StartupSource.RemovedMachine => StartupScope.LocalMachine, 
				StartupSource.RemovedMachine32 => StartupScope.LocalMachine32, 
				_ => StartupScope.CurrentUser, 
			}, out string error))
			{
				return StartupOperationResult.Fail(error);
			}
			return StartupOperationResult.Ok;
		}
		case StartupSource.RemovedFolderUser:
		case StartupSource.RemovedFolderCommon:
		{
			string path = ((entry.Source == StartupSource.RemovedFolderUser) ? UserStartupFolder : CommonStartupFolder);
			try
			{
				File.Move(Path.Combine(entry.Location, entry.Name), Path.Combine(path, entry.Name), overwrite: false);
				return StartupOperationResult.Ok;
			}
			catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
			{
				return StartupOperationResult.Fail(ex.Message);
			}
		}
		default:
			return StartupOperationResult.Ok;
		}
	}

	public bool TryRestore(string name, StartupScope scope, out string error)
	{
		string runKey = scope switch
		{
			StartupScope.LocalMachine => "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", 
			StartupScope.LocalMachine32 => "HKLM32\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", 
			_ => "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", 
		};
		string backupKey = BackupKeyFor(scope);
		return RegistryService.TryRun(_loc, () =>
		{
			object obj = _registry.ReadValue(backupKey, name);
			if (obj != null)
			{
				RegistryValueKind kind = _registry.GetValueKind(backupKey, name) ?? RegistryValueKind.String;
				_registry.WriteValue(runKey, name, obj, kind);
				_registry.DeleteValue(backupKey, name);
				StartupScope startupScope = scope;
				_registry.DeleteValue(ApprovedKeyFor(startupScope switch
				{
					StartupScope.LocalMachine => StartupSource.RunMachine, 
					StartupScope.LocalMachine32 => StartupSource.RunMachine32, 
					_ => StartupSource.RunUser, 
				}), name);
			}
		}, out error);
	}

	private static string RunKeyFor(StartupSource source)
	{
		return source switch
		{
			StartupSource.RunUser => "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", 
			StartupSource.RunMachine => "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", 
			StartupSource.RunMachine32 => "HKLM32\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", 
			_ => throw new ArgumentOutOfRangeException("source", source, null), 
		};
	}

	private static string ApprovedKeyFor(StartupSource source)
	{
		return source switch
		{
			StartupSource.RunUser => "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run", 
			StartupSource.RunMachine => "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run", 
			StartupSource.RunMachine32 => "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\Run32", 
			StartupSource.FolderUser => "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder", 
			StartupSource.FolderCommon => "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved\\StartupFolder", 
			_ => throw new ArgumentOutOfRangeException("source", source, null), 
		};
	}

	private static string BackupKeyFor(StartupScope scope)
	{
		return $"{"HKCU\\Software\\GhostEye\\StartupBackup"}\\{scope}";
	}

	private static StartupScope ScopeFor(StartupSource source)
	{
		return source switch
		{
			StartupSource.RunMachine => StartupScope.LocalMachine, 
			StartupSource.RunMachine32 => StartupScope.LocalMachine32, 
			_ => StartupScope.CurrentUser, 
		};
	}

	private static StartupSource RemovedSourceFor(StartupScope scope)
	{
		return scope switch
		{
			StartupScope.LocalMachine => StartupSource.RemovedMachine, 
			StartupScope.LocalMachine32 => StartupSource.RemovedMachine32, 
			_ => StartupSource.RemovedUser, 
		};
	}

	private string FolderBackupFor(StartupSource removedSource)
	{
		return Path.Combine(_folderBackupRoot, (removedSource == StartupSource.RemovedFolderUser) ? "FolderUser" : "FolderCommon");
	}

	private static string DisplayKey(string key)
	{
		if (!key.StartsWith("HKLM32\\", StringComparison.OrdinalIgnoreCase))
		{
			return key;
		}
		return "HKLM\\SOFTWARE\\WOW6432Node\\" + key.Substring("HKLM32\\Software\\".Length);
	}

	private IReadOnlyList<string> SafeValueNames(string key)
	{
		try
		{
			return _registry.GetValueNames(key);
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException) ? true : false)
		{
			return Array.Empty<string>();
		}
	}

	private static string? FirstLine(string text)
	{
		return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
	}
}
