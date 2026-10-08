using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;

namespace GhostEye.Infrastructure.Platform;

public sealed class GameBoostService(IPowerPlanService powerPlans, IRegistryService registry, IPerformanceToolsService tools, ILocalizer loc, string statePath) : IGameBoostService
{
	private static readonly string[] UpdateServices = new string[4] { "wuauserv", "UsoSvc", "DoSvc", "BITS" };

	private static readonly BoostApp[] Candidates = new BoostApp[26]
	{
		new BoostApp("OneDrive", "OneDrive"),
		new BoostApp("Dropbox", "Dropbox"),
		new BoostApp("GoogleDriveFS", "Google Drive"),
		new BoostApp("ms-teams", "Microsoft Teams"),
		new BoostApp("Teams", "Microsoft Teams (classic)"),
		new BoostApp("Skype", "Skype"),
		new BoostApp("Slack", "Slack"),
		new BoostApp("Zoom", "Zoom"),
		new BoostApp("Spotify", "Spotify"),
		new BoostApp("Creative Cloud", "Adobe Creative Cloud"),
		new BoostApp("CCXProcess", "Adobe CCX"),
		new BoostApp("AdobeCollabSync", "Adobe Acrobat sync"),
		new BoostApp("EpicGamesLauncher", "Epic Games Launcher"),
		new BoostApp("EADesktop", "EA app"),
		new BoostApp("upc", "Ubisoft Connect"),
		new BoostApp("GalaxyClient", "GOG Galaxy"),
		new BoostApp("Battle.net", "Battle.net"),
		new BoostApp("PhoneExperienceHost", "Phone Link"),
		new BoostApp("WhatsApp", "WhatsApp"),
		new BoostApp("Telegram", "Telegram"),
		new BoostApp("iCloudServices", "iCloud"),
		new BoostApp("msedge", "Microsoft Edge", SelectedByDefault: false),
		new BoostApp("chrome", "Google Chrome", SelectedByDefault: false),
		new BoostApp("firefox", "Firefox", SelectedByDefault: false),
		new BoostApp("brave", "Brave", SelectedByDefault: false),
		new BoostApp("opera", "Opera", SelectedByDefault: false)
	};

	private const string UltimatePlanKey = "HKCU\\Software\\GhostEye";

	private static readonly Guid UltimateTemplate = new Guid("e9a42b02-d5df-448d-aa00-03f14749eb61");

	public BoostState? State => Load();

	public bool IsActive => (object)State != null;

	public IReadOnlyList<BoostApp> GetClosableApps()
	{
		HashSet<string> running = Process.GetProcesses().Select((Process p) =>
		{
			using (p)
			{
				return p.ProcessName;
			}
		}).ToHashSet(StringComparer.OrdinalIgnoreCase);
		return (from c in Candidates
			where running.Contains(c.ProcessName)
			group c by c.DisplayName into g
			select g.First()).ToList();
	}

	public async Task<BoostReport> StartAsync(BoostOptions options, CancellationToken ct = default(CancellationToken))
	{
		if (IsActive)
		{
			return new BoostReport(Success: true, 0, 0L, null, new _003C_003Ez__ReadOnlySingleElementList<string>(loc["boost.alreadyActive"]));
		}
		List<string> warnings = new List<string>();
		BoostState boostState = new BoostState
		{
			StartedAt = DateTimeOffset.Now,
			AutoGameName = options.AutoGameName,
			AutoProcessId = options.AutoProcessId
		};
		string planName = null;
		if (options.SwitchPowerPlan)
		{
			Guid? activeScheme = powerPlans.GetActiveScheme();
			if (activeScheme.HasValue)
			{
				Guid valueOrDefault = activeScheme.GetValueOrDefault();
				Guid? guid = BestPlan();
				if (guid.HasValue && guid != valueOrDefault)
				{
					if (powerPlans.TrySetActiveScheme(guid.Value, out string error))
					{
						boostState = boostState with
						{
							PreviousPowerPlan = valueOrDefault
						};
						planName = powerPlans.GetSchemeName(guid.Value);
					}
					else
					{
						warnings.Add(error);
					}
				}
				else
				{
					planName = powerPlans.GetSchemeName(valueOrDefault);
				}
			}
		}
		Save(boostState);
		if (options.PauseUpdates)
		{
			List<string> list = new List<string>();
			string[] updateServices = UpdateServices;
			foreach (string text in updateServices)
			{
				try
				{
					using ServiceController serviceController = new ServiceController(text);
					ServiceControllerStatus status = serviceController.Status;
					if ((status == ServiceControllerStatus.StartPending || status == ServiceControllerStatus.Running) ? true : false)
					{
						serviceController.Stop();
						serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(20.0));
						list.Add(text);
					}
				}
				catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception || ex is System.ServiceProcess.TimeoutException) ? true : false)
				{
				}
			}
			if (list.Count == 0)
			{
				warnings.Add(loc["boost.updatesNotPaused"]);
			}
			boostState = boostState with
			{
				StoppedServices = list
			};
			Save(boostState);
		}
		List<string> closed = new List<string>();
		foreach (BoostApp item in Candidates.Where((BoostApp c) => options.AppsToClose.Contains(c.ProcessName, StringComparer.OrdinalIgnoreCase)))
		{
			Process[] processesByName = Process.GetProcessesByName(item.ProcessName);
			foreach (Process process in processesByName)
			{
				using (process)
				{
					try
					{
						string path = process.MainModule?.FileName;
						if (process.MainWindowHandle == IntPtr.Zero)
						{
							goto IL_036a;
						}
						process.CloseMainWindow();
						if (!process.WaitForExit(2000))
						{
							goto IL_036a;
						}
						AddClosed(path);
						goto end_IL_0322;
						IL_036a:
						process.Kill(entireProcessTree: true);
						AddClosed(path);
						end_IL_0322:;
					}
					catch (Exception ex2) when ((ex2 is Win32Exception || ex2 is InvalidOperationException || ex2 is NotSupportedException) ? true : false)
					{
					}
				}
			}
		}
		boostState = boostState with
		{
			ClosedExecutables = closed
		};
		Save(boostState);
		long freedBytes = 0L;
		if (options.FreeMemory)
		{
			await Task.Delay(1000, ct).ConfigureAwait(continueOnCapturedContext: false);
			freedBytes = (await tools.FreeMemoryAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).FreedBytes;
		}
		return new BoostReport(Success: true, closed.Count, freedBytes, planName, warnings);
		void AddClosed(string? text2)
		{
			if (text2 != null && !closed.Contains(text2, StringComparer.OrdinalIgnoreCase))
			{
				closed.Add(text2);
			}
		}
	}

	public Task<BoostReport> StopAsync(bool reopenApps, CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			BoostState boostState = Load();
			if ((object)boostState == null)
			{
				return new BoostReport(Success: true, 0, 0L, null, Array.Empty<string>());
			}
			List<string> list = new List<string>();
			Guid? previousPowerPlan = boostState.PreviousPowerPlan;
			if (previousPowerPlan.HasValue)
			{
				Guid valueOrDefault = previousPowerPlan.GetValueOrDefault();
				if (!powerPlans.TrySetActiveScheme(valueOrDefault, out string error))
				{
					list.Add(error);
				}
			}
			foreach (string stoppedService in boostState.StoppedServices)
			{
				try
				{
					using ServiceController serviceController = new ServiceController(stoppedService);
					if (serviceController.Status == ServiceControllerStatus.Stopped)
					{
						serviceController.Start();
					}
				}
				catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception) ? true : false)
				{
				}
			}
			int num = 0;
			if (reopenApps)
			{
				foreach (string item in boostState.ClosedExecutables.Where(File.Exists))
				{
					try
					{
						Process.Start(new ProcessStartInfo(item)
						{
							UseShellExecute = true,
							WorkingDirectory = (Path.GetDirectoryName(item) ?? string.Empty),
							WindowStyle = ProcessWindowStyle.Minimized
						});
						num++;
					}
					catch (Exception ex2) when ((ex2 is Win32Exception || ex2 is InvalidOperationException) ? true : false)
					{
					}
				}
			}
			TryDelete();
			bool success = list.Count == 0;
			int closedApps = num;
			long freedBytes = 0L;
			previousPowerPlan = boostState.PreviousPowerPlan;
			object powerPlan;
			if (previousPowerPlan.HasValue)
			{
				Guid valueOrDefault2 = previousPowerPlan.GetValueOrDefault();
				powerPlan = powerPlans.GetSchemeName(valueOrDefault2);
			}
			else
			{
				powerPlan = null;
			}
			return new BoostReport(success, closedApps, freedBytes, (string?)powerPlan, list);
		}, ct);
	}

	private Guid? BestPlan()
	{
		IReadOnlyList<PowerPlan> schemes = powerPlans.GetSchemes();
		if (schemes.Any((PowerPlan s) => s.Id == UltimateTemplate))
		{
			return UltimateTemplate;
		}
		if (Guid.TryParse(registry.ReadString("HKCU\\Software\\GhostEye", "UltimatePlanGuid"), out var ultimate) && schemes.Any((PowerPlan s) => s.Id == ultimate))
		{
			return ultimate;
		}
		if (!powerPlans.IsHighPerformanceAvailable())
		{
			return null;
		}
		return powerPlans.HighPerformanceGuid;
	}

	private BoostState? Load()
	{
		try
		{
			return File.Exists(statePath) ? JsonSerializer.Deserialize<BoostState>(File.ReadAllText(statePath)) : null;
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? true : false)
		{
			return null;
		}
	}

	private void Save(BoostState state)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(statePath));
			File.WriteAllText(statePath, JsonSerializer.Serialize(state));
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
	}

	private void TryDelete()
	{
		try
		{
			File.Delete(statePath);
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
	}
}
