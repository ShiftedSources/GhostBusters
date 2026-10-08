using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Presets;

namespace GhostEye.Infrastructure.Platform;

public sealed class ServiceBackupService(IServiceManagerService services, ISystemOptimizerService optimizer, IProcessRunner processRunner, string directory) : IServiceBackupService
{
	private const int MaxAutomatic = 10;

	private const string IdFormat = "yyyyMMdd-HHmmss-fff";

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	public IReadOnlyList<ServiceBackup> List()
	{
		if (!Directory.Exists(directory))
		{
			return Array.Empty<ServiceBackup>();
		}
		List<ServiceBackup> list = new List<ServiceBackup>();
		foreach (string item in Directory.EnumerateFiles(directory, "services-*.json"))
		{
			try
			{
				ServiceBackup serviceBackup = JsonSerializer.Deserialize<ServiceBackup>(File.ReadAllText(item), JsonOptions);
				if ((object)serviceBackup != null)
				{
					list.Add(serviceBackup);
				}
			}
			catch (Exception ex) when ((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? true : false)
			{
			}
		}
		return list.OrderByDescending((ServiceBackup b) => b.CreatedAt).ToList();
	}

	public async Task<ServiceBackup> CreateAsync(bool automatic, CancellationToken ct = default(CancellationToken))
	{
		IReadOnlyList<OptionalServiceState> serviceStates = await Task.Run((Func<IReadOnlyList<OptionalServiceState>>)optimizer.GetServices, ct).ConfigureAwait(continueOnCapturedContext: false);
		IReadOnlyList<OptionalTaskState> source = await optimizer.GetTasksAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		DateTime now = DateTime.Now;
		ServiceBackup backup = new ServiceBackup(now.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture) + "-" + Guid.NewGuid().ToString("N").Substring(0, 6), now, automatic, (from s in serviceStates
			where s.IsInstalled && s.StartupMode.HasValue
			select new ServiceSnapshotEntry(s.Definition.Name, s.StartupMode.Value, s.Status == ServiceControllerStatus.Running)).ToList(), (from t in source
			where t.Exists
			select new TaskSnapshotEntry(t.Definition.Path, t.IsEnabled)).ToList());
		Directory.CreateDirectory(directory);
		await File.WriteAllTextAsync(PathFor(backup), JsonSerializer.Serialize(backup, JsonOptions), ct).ConfigureAwait(continueOnCapturedContext: false);
		if (automatic)
		{
			foreach (ServiceBackup item in (from b in List()
				where b.IsAutomatic
				select b).Skip(10))
			{
				Delete(item);
			}
		}
		return backup;
	}

	public async Task<ServiceRestoreResult> RestoreAsync(ServiceBackup backup, CancellationToken ct = default(CancellationToken))
	{
		int restored = 0;
		List<string> failed = new List<string>();
		foreach (ServiceSnapshotEntry entry in backup.Services)
		{
			ct.ThrowIfCancellationRequested();
			if (await Task.Run(() => RestoreService(entry), ct).ConfigureAwait(continueOnCapturedContext: false))
			{
				restored++;
			}
			else
			{
				failed.Add(entry.Name);
			}
		}
		IReadOnlyList<OptionalTaskState> tasks = await optimizer.GetTasksAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		foreach (TaskSnapshotEntry entry2 in backup.Tasks)
		{
			OptionalTaskState optionalTaskState = tasks.FirstOrDefault((OptionalTaskState t) => string.Equals(t.Definition.Path, entry2.Path, StringComparison.OrdinalIgnoreCase));
			if ((object)optionalTaskState == null || !optionalTaskState.Exists)
			{
				failed.Add(entry2.Path);
			}
			else if (optionalTaskState.IsEnabled == entry2.Enabled)
			{
				restored++;
			}
			else if ((await processRunner.RunAsync("schtasks.exe", "/Change /TN \"" + entry2.Path + "\" " + (entry2.Enabled ? "/ENABLE" : "/DISABLE"), ct).ConfigureAwait(continueOnCapturedContext: false)).Success)
			{
				restored++;
			}
			else
			{
				failed.Add(entry2.Path);
			}
		}
		return new ServiceRestoreResult(restored, failed);
	}

	public bool Delete(ServiceBackup backup)
	{
		try
		{
			File.Delete(PathFor(backup));
			return true;
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			return false;
		}
	}

	private bool RestoreService(ServiceSnapshotEntry entry)
	{
		if (!ServiceCatalog.Services.Any((ServiceTweakDefinition s) => s.Name == entry.Name) || !services.GetStartupMode(entry.Name).HasValue)
		{
			return false;
		}
		if (services.GetStartupMode(entry.Name) != entry.StartupMode && !services.TrySetStartupMode(entry.Name, entry.StartupMode, out string error))
		{
			return false;
		}
		if (entry.StartupMode == ServiceStartupMode.Disabled)
		{
			services.TryStop(entry.Name, out error);
		}
		else if (entry.WasRunning)
		{
			services.TryStart(entry.Name, out error);
		}
		return services.GetStartupMode(entry.Name) == entry.StartupMode;
	}

	private string PathFor(ServiceBackup backup)
	{
		string text = string.Concat(backup.Id.Where((char c) => char.IsAsciiLetterOrDigit(c) || c == '-'));
		return Path.Combine(directory, "services-" + text + ".json");
	}
}
