using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;
using GhostEye.Core.Presets;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Apps;

public sealed class InstalledAppsService(IRegistryService registry, IProcessRunner processRunner, ILocalizer loc) : IInstalledAppsService
{
	private static readonly string[] UninstallRoots = new string[3] { "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", "HKLM32\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", "HKCU\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall" };

	private static readonly TimeSpan UninstallTimeout = TimeSpan.FromMinutes(15.0);

	public Task<IReadOnlyList<InstalledApp>> GetDesktopAppsAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.Run((Func<IReadOnlyList<InstalledApp>>)(() =>
		{
			UsageHistory usage = UsageHistory.Collect(registry);
			Dictionary<string, InstalledApp> dictionary = new Dictionary<string, InstalledApp>(StringComparer.OrdinalIgnoreCase);
			string[] uninstallRoots = UninstallRoots;
			foreach (string text in uninstallRoots)
			{
				foreach (string item in SafeSubKeys(text))
				{
					ct.ThrowIfCancellationRequested();
					string text2 = text + "\\" + item;
					string text3 = registry.ReadString(text2, "DisplayName")?.Trim();
					string text4 = registry.ReadString(text2, "UninstallString")?.Trim();
					bool flag = string.IsNullOrEmpty(text3) || string.IsNullOrEmpty(text4) || registry.ReadDword(text2, "SystemComponent") == 1 || !string.IsNullOrEmpty(registry.ReadString(text2, "ParentKeyName"));
					if (!flag)
					{
						bool flag2;
						switch (registry.ReadString(text2, "ReleaseType"))
						{
						case "Update":
						case "Hotfix":
						case "Security Update":
							flag2 = true;
							break;
						default:
							flag2 = false;
							break;
						}
						flag = flag2;
					}
					if (!flag)
					{
						string installLocation = registry.ReadString(text2, "InstallLocation")?.Trim().Trim('"') ?? string.Empty;
						string? text5 = registry.ReadString(text2, "DisplayIcon");
						string text6 = ((text5 != null) ? text5.Split(',')[0].Trim().Trim('"') : null);
						int valueOrDefault = registry.ReadDword(text2, "EstimatedSize").GetValueOrDefault();
						InstalledApp installedApp = new InstalledApp
						{
							Key = text2,
							Name = CleanText(text3),
							Publisher = CleanText(registry.ReadString(text2, "Publisher")),
							Version = CleanText(registry.ReadString(text2, "DisplayVersion")),
							SizeBytes = Math.Max(0L, valueOrDefault) * 1024,
							InstalledOn = ParseInstallDate(registry.ReadString(text2, "InstallDate")),
							InstallLocation = installLocation,
							UninstallCommand = text4,
							QuietUninstallCommand = registry.ReadString(text2, "QuietUninstallString")?.Trim(),
							IsMachineWide = !text.StartsWith("HKCU", StringComparison.Ordinal),
							IconPath = ((text6 != null && text6.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) ? text6 : null)
						};
						installedApp = installedApp with
						{
							LastUsed = FindLastUsed(installedApp, usage)
						};
						dictionary.TryAdd(text3 + "|" + installedApp.Version, installedApp);
					}
				}
			}
			return dictionary.Values.OrderBy((InstalledApp a) => a.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
		}), ct);
	}

	public async Task<OperationResult> UninstallAsync(InstalledApp app, CancellationToken ct = default(CancellationToken))
	{
		var (fileName, arguments) = BuildUninstallCommand(app);
		try
		{
			using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
			timeout.CancelAfter(UninstallTimeout);
			using Process process = Process.Start(new ProcessStartInfo(fileName, arguments)
			{
				UseShellExecute = true
			});
			if (process == null)
			{
				return OperationResult.Fail(loc["apps.error.start"]);
			}
			await process.WaitForExitAsync(timeout.Token).ConfigureAwait(continueOnCapturedContext: false);
			int exitCode = process.ExitCode;
			bool flag = ((exitCode == 0 || exitCode == 1641 || exitCode == 3010) ? true : false);
			if (flag || !registry.KeyExists(app.Key))
			{
				return OperationResult.Ok;
			}
			return OperationResult.Fail(loc.Format("apps.error.exitCode", process.ExitCode));
		}
		catch (OperationCanceledException) when (!ct.IsCancellationRequested)
		{
			return OperationResult.Fail(loc["apps.error.timeout"]);
		}
		catch (Exception ex2) when ((ex2 is Win32Exception || ex2 is InvalidOperationException) ? true : false)
		{
			return OperationResult.Fail(ex2.Message);
		}
	}

	public static (string File, string Arguments) BuildUninstallCommand(InstalledApp app)
	{
		Match match = MsiProductCode().Match(app.UninstallCommand);
		if (app.UninstallCommand.Contains("msiexec", StringComparison.OrdinalIgnoreCase) && match.Success)
		{
			return (File: "msiexec.exe", Arguments: "/x " + match.Value + " /qb! /norestart");
		}
		string text = Environment.ExpandEnvironmentVariables(string.IsNullOrWhiteSpace(app.QuietUninstallCommand) ? app.UninstallCommand : app.QuietUninstallCommand);
		if (text.StartsWith('"'))
		{
			int num = text.IndexOf('"', 1);
			if (num > 1)
			{
				return (File: text.Substring(1, num - 1), Arguments: text.Substring(num + 1).Trim());
			}
		}
		int num2 = text.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
		if (num2 > 0)
		{
			return (File: text.Substring(0, num2 + 4), Arguments: text.Substring(num2 + 4).Trim());
		}
		return (File: text, Arguments: string.Empty);
	}

	public async Task<IReadOnlyList<StoreApp>> GetBloatwareAsync(CancellationToken ct = default(CancellationToken))
	{
		ProcessResult processResult = await processRunner.RunPowerShellAsync("[Console]::OutputEncoding=[Text.Encoding]::UTF8;Get-AppxPackage | Where-Object { -not $_.IsFramework -and $_.SignatureKind -ne 'System' } | Select-Object @{n='N';e={$_.Name}}, @{n='F';e={$_.PackageFullName}}, @{n='V';e={[string]$_.Version}} | ConvertTo-Json -Compress", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!processResult.Success || string.IsNullOrWhiteSpace(processResult.StandardOutput))
		{
			return Array.Empty<StoreApp>();
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(processResult.StandardOutput.Trim());
			List<JsonElement> obj = ((jsonDocument.RootElement.ValueKind == JsonValueKind.Array) ? jsonDocument.RootElement.EnumerateArray().ToList() : new List<JsonElement>(1) { jsonDocument.RootElement });
			List<StoreApp> list = new List<StoreApp>();
			foreach (JsonElement item in obj)
			{
				string name = item.GetProperty("N").GetString() ?? string.Empty;
				BloatwareDefinition bloatwareDefinition = BloatwareCatalog.Apps.FirstOrDefault((BloatwareDefinition d) => name.Equals(d.PackageName, StringComparison.OrdinalIgnoreCase));
				if ((object)bloatwareDefinition != null)
				{
					list.Add(new StoreApp(bloatwareDefinition, item.GetProperty("F").GetString() ?? string.Empty, loc["bloat." + bloatwareDefinition.Key + ".name"], item.GetProperty("V").GetString() ?? string.Empty));
				}
			}
			return list.OrderBy((StoreApp a) => a.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
		}
		catch (Exception ex) when ((ex is JsonException || ex is KeyNotFoundException || ex is InvalidOperationException) ? true : false)
		{
			return Array.Empty<StoreApp>();
		}
	}

	public async Task<OperationResult> RemoveStoreAppAsync(StoreApp app, CancellationToken ct = default(CancellationToken))
	{
		if (!PackageNamePattern().IsMatch(app.PackageFullName))
		{
			return OperationResult.Fail(loc["apps.error.start"]);
		}
		ProcessResult processResult = await processRunner.RunPowerShellAsync("Remove-AppxPackage -Package '" + app.PackageFullName + "' -ErrorAction Stop", ct).ConfigureAwait(continueOnCapturedContext: false);
		return processResult.Success ? OperationResult.Ok : OperationResult.Fail(FirstLine(processResult.StandardError) ?? loc["apps.error.start"]);
	}

	private static DateTime? FindLastUsed(InstalledApp app, UsageHistory usage)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		if (!string.IsNullOrEmpty(app.InstallLocation))
		{
			list.Add(app.InstallLocation);
		}
		if (!string.IsNullOrEmpty(app.IconPath))
		{
			list2.Add(app.IconPath);
			string directoryName = Path.GetDirectoryName(app.IconPath);
			if (directoryName != null)
			{
				list.Add(directoryName);
			}
		}
		string item = BuildUninstallCommand(app).File;
		if (Path.IsPathRooted(item))
		{
			string directoryName2 = Path.GetDirectoryName(item);
			if (directoryName2 != null)
			{
				list.Add(directoryName2);
			}
		}
		return usage.LastUsed(list, list2);
	}

	private static string CleanText(string? value)
	{
		if (value != null)
		{
			return WhitespaceRun().Replace(new string(value.Where((char c) => !char.IsControl(c)).ToArray()), " ").Trim();
		}
		return string.Empty;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex WhitespaceRun()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__WhitespaceRun_0.Instance;
	}

	private static DateTime? ParseInstallDate(string? value)
	{
		if (!DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
		{
			return null;
		}
		return result;
	}

	private IReadOnlyList<string> SafeSubKeys(string root)
	{
		try
		{
			return registry.GetSubKeyNames(root);
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

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex MsiProductCode()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__MsiProductCode_1.Instance;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex PackageNamePattern()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__PackageNamePattern_2.Instance;
	}
}
