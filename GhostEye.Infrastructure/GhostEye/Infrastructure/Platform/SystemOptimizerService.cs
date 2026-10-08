using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;
using GhostEye.Core.Presets;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Platform;

public sealed class SystemOptimizerService(IServiceManagerService services, IRegistryService registry, IProcessRunner processRunner, ILocalizer loc) : ISystemOptimizerService
{
	private const string BackupKey = "HKCU\\Software\\GhostEye\\ServiceBackup";

	private string NotAppliedMessage => loc[Elevation.IsElevated ? "services.error.notAppliedElevated" : "services.error.notApplied"];

	public IReadOnlyList<OptionalServiceState> GetServices()
	{
		return ServiceCatalog.Services.Select((ServiceTweakDefinition definition) =>
		{
			ServiceInfo service = services.GetService(definition.Name);
			ServiceControllerStatus? status = null;
			try
			{
				status = service?.Status;
			}
			catch (InvalidOperationException)
			{
			}
			return new OptionalServiceState(definition, (object)service != null, service?.DisplayName ?? definition.Name, status, ((object)service == null) ? ((ServiceStartupMode?)null) : services.GetStartupMode(definition.Name));
		}).ToList();
	}

	public OperationResult SetServiceOptimized(ServiceTweakDefinition definition, bool optimized)
	{
		ServiceStartupMode? current = services.GetStartupMode(definition.Name);
		if (!current.HasValue)
		{
			return OperationResult.Fail(loc["services.error.notInstalled"]);
		}
		string error;
		ServiceStartupMode serviceStartupMode;
		string error2;
		if (optimized)
		{
			if (!registry.ReadDword("HKCU\\Software\\GhostEye\\ServiceBackup", definition.Name).HasValue)
			{
				RegistryService.TryRun(loc, () =>
				{
					registry.WriteValue("HKCU\\Software\\GhostEye\\ServiceBackup", definition.Name, (int)current.Value, RegistryValueKind.DWord);
				}, out error);
			}
			serviceStartupMode = (definition.Disable ? ServiceStartupMode.Disabled : ServiceStartupMode.Manual);
			if (!services.TrySetStartupMode(definition.Name, serviceStartupMode, out error2))
			{
				return OperationResult.Fail(error2);
			}
			if (definition.Disable)
			{
				services.TryStop(definition.Name, out error);
			}
		}
		else
		{
			int? num = registry.ReadDword("HKCU\\Software\\GhostEye\\ServiceBackup", definition.Name);
			serviceStartupMode = ((!num.HasValue) ? ServiceStartupMode.Manual : ((ServiceStartupMode)num.Value));
			if (!services.TrySetStartupMode(definition.Name, serviceStartupMode, out error2))
			{
				return OperationResult.Fail(error2);
			}
			RegistryService.TryRun(loc, () =>
			{
				registry.DeleteValue("HKCU\\Software\\GhostEye\\ServiceBackup", definition.Name);
			}, out error);
			if (serviceStartupMode == ServiceStartupMode.Automatic)
			{
				services.TryStart(definition.Name, out error);
			}
		}
		if (services.GetStartupMode(definition.Name) != serviceStartupMode)
		{
			return OperationResult.Fail(NotAppliedMessage);
		}
		return OperationResult.Ok;
	}

	private static bool IsAccessDenied(string output)
	{
		if (!output.Contains("0x80070005", StringComparison.OrdinalIgnoreCase) && !output.Contains("Access is denied", StringComparison.OrdinalIgnoreCase))
		{
			return output.Contains("Accesso negato", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public async Task<IReadOnlyList<OptionalTaskState>> GetTasksAsync(CancellationToken ct = default(CancellationToken))
	{
		string text = string.Join(",", ServiceCatalog.Tasks.Select((TaskTweakDefinition t) => "'" + t.Path.Replace("'", "''") + "'"));
		string command = "[Console]::OutputEncoding=[Text.Encoding]::UTF8;$r = foreach ($p in @(" + text + ")) { $i = $p.LastIndexOf('\\'); $t = Get-ScheduledTask -TaskPath $p.Substring(0, $i + 1) -TaskName $p.Substring($i + 1) -ErrorAction SilentlyContinue; [pscustomobject]@{ P = $p; E = [bool]$t; S = if ($t) { [string]$t.State } else { '' } } }; $r | ConvertTo-Json -Compress";
		Dictionary<string, (bool Exists, bool Enabled)> states = new Dictionary<string, (bool, bool)>(StringComparer.OrdinalIgnoreCase);
		try
		{
			ProcessResult processResult = await processRunner.RunPowerShellAsync(command, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (processResult.Success && !string.IsNullOrWhiteSpace(processResult.StandardOutput))
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(processResult.StandardOutput.Trim());
				foreach (JsonElement item in (jsonDocument.RootElement.ValueKind == JsonValueKind.Array) ? jsonDocument.RootElement.EnumerateArray().ToList() : new List<JsonElement>(1) { jsonDocument.RootElement })
				{
					string key = item.GetProperty("P").GetString() ?? string.Empty;
					bool boolean = item.GetProperty("E").GetBoolean();
					string text2 = item.GetProperty("S").GetString() ?? string.Empty;
					states[key] = (boolean, !text2.Equals("Disabled", StringComparison.OrdinalIgnoreCase));
				}
			}
		}
		catch (Exception ex) when ((ex is JsonException || ex is InvalidOperationException || ex is KeyNotFoundException || ex is Win32Exception) ? true : false)
		{
		}
		return ServiceCatalog.Tasks.Select((TaskTweakDefinition t) => (!states.TryGetValue(t.Path, out (bool, bool) value)) ? new OptionalTaskState(t, Exists: false, IsEnabled: false) : new OptionalTaskState(t, value.Item1, value.Item2)).ToList();
	}

	public async Task<OperationResult> SetTaskOptimizedAsync(TaskTweakDefinition definition, bool optimized, CancellationToken ct = default(CancellationToken))
	{
		ProcessResult processResult = await processRunner.RunAsync("schtasks.exe", "/Change /TN \"" + definition.Path + "\" " + (optimized ? "/DISABLE" : "/ENABLE"), ct).ConfigureAwait(continueOnCapturedContext: false);
		if (processResult.Success)
		{
			return OperationResult.Ok;
		}
		string text = processResult.Combined.Trim();
		if (Elevation.IsElevated && IsAccessDenied(text))
		{
			return OperationResult.Fail(loc["services.error.protected"]);
		}
		return OperationResult.Fail((text.Length > 0) ? text : NotAppliedMessage);
	}
}
