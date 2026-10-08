using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using GhostEye.Core.Localization;
using GhostEye.Core.Services;
using GhostEye.Infrastructure.Cleanup;
using GhostEye.Infrastructure.Games;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.Services;

public static class BackgroundAgent
{
	private static readonly HashSet<string> MaintenanceCategories = new HashSet<string>(StringComparer.Ordinal) { "temp-user", "temp-windows", "crash-dump", "logs", "error-reports" };

	private static readonly HashSet<string> Launchers = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "EpicGamesLauncher", "EADesktop", "upc", "GalaxyClient", "Battle.net" };

	private static readonly TimeSpan DriftInterval = TimeSpan.FromHours(24.0);

	private static readonly TimeSpan LibraryRefresh = TimeSpan.FromMinutes(10.0);

	private static DispatcherTimer? _slowTimer;

	private static DispatcherTimer? _gameTimer;

	private static IReadOnlyList<GameEntry> _library = Array.Empty<GameEntry>();

	private static DateTimeOffset _libraryLoadedAt = DateTimeOffset.MinValue;

	private static HashSet<string> _lastNotifiedDrift = new HashSet<string>(StringComparer.Ordinal);

	private static bool _busy;

	private static bool _gameBusy;

	public static DriftReport LatestDrift { get; private set; } = DriftReport.Empty;

	public static event EventHandler? DriftChanged;

	public static event EventHandler? BoostChanged;

	public static void Start()
	{
		_slowTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromMinutes(5.0)
		};
		_slowTimer.Tick += async (object? _, EventArgs _) =>
		{
			await SlowTickAsync();
		};
		_slowTimer.Start();
		_gameTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(5.0)
		};
		_gameTimer.Tick += async (object? _, EventArgs _) =>
		{
			await GameTickAsync();
		};
		_gameTimer.Start();
		DispatcherTimer first = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(20.0)
		};
		first.Tick += async (object? _, EventArgs _) =>
		{
			first.Stop();
			await SlowTickAsync();
		};
		first.Start();
	}

	public static void Stop()
	{
		_slowTimer?.Stop();
		_gameTimer?.Stop();
	}

	private static async Task SlowTickAsync()
	{
		if (_busy)
		{
			return;
		}
		_busy = true;
		try
		{
			AppSettings current = AppServices.Settings.Current;
			if (current.MaintenanceEnabled && IsDue(current.LastMaintenance, TimeSpan.FromDays(Math.Max(1, current.MaintenanceIntervalDays))))
			{
				MaintenanceReport maintenanceReport = await RunMaintenanceAsync().ConfigureAwait(continueOnCapturedContext: true);
				ILocalizer localizer = AppServices.Localizer;
				TrayIcon.Notify(localizer["maintenance.notify.title"], (maintenanceReport.DriftedTweaks > 0) ? localizer.Format("maintenance.notify.bodyDrift", GhostFormat.Bytes(maintenanceReport.FreedBytes), maintenanceReport.DriftedTweaks) : localizer.Format("maintenance.notify.body", GhostFormat.Bytes(maintenanceReport.FreedBytes)), (maintenanceReport.DriftedTweaks > 0) ? "dashboard" : null);
			}
			else if (IsDue(current.LastDriftCheck, DriftInterval))
			{
				await CheckDriftAsync(notifyWhenClean: false).ConfigureAwait(continueOnCapturedContext: true);
			}
		}
		catch (Exception exception)
		{
			AppServices.Logger.Error("Background check failed", exception);
		}
		finally
		{
			_busy = false;
		}
	}

	private static bool IsDue(DateTimeOffset? last, TimeSpan interval)
	{
		if (last.HasValue)
		{
			return DateTimeOffset.Now - last.Value >= interval;
		}
		return true;
	}

	public static async Task<DriftReport> CheckDriftAsync(bool notifyWhenClean)
	{
		DriftReport driftReport = await AppServices.Drift.CheckAsync().ConfigureAwait(continueOnCapturedContext: true);
		IReadOnlyList<string> driftDismissed = AppServices.Settings.Current.DriftDismissed;
		List<string> stillDismissed = driftDismissed.Where(((IEnumerable<string>)driftReport.DriftedTweakIds).Contains<string>).ToList();
		AppServices.Settings.Update((AppSettings s) => s with
		{
			LastDriftCheck = DateTimeOffset.Now,
			DriftDismissed = stillDismissed
		});
		List<string> list = driftReport.DriftedTweakIds.Where((string id) => !stillDismissed.Contains(id)).ToList();
		LatestDrift = driftReport with
		{
			DriftedTweakIds = list
		};
		DriftChanged?.Invoke(null, EventArgs.Empty);
		ILocalizer localizer = AppServices.Localizer;
		if (list.Count > 0 && !_lastNotifiedDrift.SetEquals(list))
		{
			_lastNotifiedDrift = list.ToHashSet(StringComparer.Ordinal);
			TrayIcon.Notify(localizer["drift.notify.title"], localizer.Format("drift.notify.body", list.Count), "dashboard");
		}
		else if ((list.Count == 0) & notifyWhenClean)
		{
			TrayIcon.Notify(localizer["drift.notify.title"], localizer["drift.notify.clean"]);
		}
		return LatestDrift;
	}

	public static void DismissDrift(IEnumerable<string> tweakIds)
	{
		List<string> ids = tweakIds.ToList();
		AppServices.Settings.Update((AppSettings s) => s with
		{
			DriftDismissed = s.DriftDismissed.Union(ids).ToList()
		});
		LatestDrift = LatestDrift with
		{
			DriftedTweakIds = LatestDrift.DriftedTweakIds.Except(ids).ToList()
		};
		DriftChanged?.Invoke(null, EventArgs.Empty);
	}

	public static void RefreshAfterHistoryChange()
	{
		RefreshDriftFromHistoryAsync();
		static async Task RefreshDriftFromHistoryAsync()
		{
			IReadOnlyList<string> readOnlyList = await AppServices.Drift.GetActiveTweakIdsAsync().ConfigureAwait(continueOnCapturedContext: true);
			LatestDrift = LatestDrift with
			{
				ActiveTweakIds = readOnlyList,
				DriftedTweakIds = LatestDrift.DriftedTweakIds.Where(((IEnumerable<string>)readOnlyList).Contains<string>).ToList()
			};
			DriftChanged?.Invoke(null, EventArgs.Empty);
		}
	}

	public static void ClearDrift(IEnumerable<string> fixedIds)
	{
		LatestDrift = LatestDrift with
		{
			DriftedTweakIds = LatestDrift.DriftedTweakIds.Except(fixedIds).ToList()
		};
		DriftChanged?.Invoke(null, EventArgs.Empty);
	}

	public static async Task<MaintenanceReport> RunMaintenanceAsync()
	{
		AppServices.Logger.Info("Maintenance started.");
		List<CleanupCategory> list = (await AppServices.Cleanup.ScanAsync().ConfigureAwait(continueOnCapturedContext: true)).Categories.Where((CleanupCategory c) => MaintenanceCategories.Contains(c.Id) && c.Bytes > 0 && c.Warning == null).ToList();
		CleanupOutcome cleanupOutcome = ((list.Count <= 0) ? new CleanupOutcome
		{
			FreedBytes = 0L,
			DeletedFiles = 0,
			SkippedFiles = 0
		} : (await AppServices.Cleanup.CleanAsync(list).ConfigureAwait(continueOnCapturedContext: true)));
		CleanupOutcome outcome = cleanupOutcome;
		DriftReport driftReport = await CheckDriftAsync(notifyWhenClean: false).ConfigureAwait(continueOnCapturedContext: true);
		AppServices.Settings.Update((AppSettings s) => s with
		{
			LastMaintenance = DateTimeOffset.Now,
			LastMaintenanceFreedBytes = outcome.FreedBytes
		});
		AppServices.Logger.Info($"Maintenance done: {outcome.FreedBytes} bytes freed, {outcome.DeletedFiles} files, {driftReport.DriftedTweakIds.Count} drifted.");
		return new MaintenanceReport(outcome.FreedBytes, outcome.DeletedFiles, driftReport.DriftedTweakIds.Count);
	}

	public static IReadOnlyList<GameEntry> GetLibrary(bool refresh = false)
	{
		if (refresh || DateTimeOffset.Now - _libraryLoadedAt > LibraryRefresh)
		{
			_library = AppServices.Games.Detect(AppServices.Settings.Current.CustomGames);
			_libraryLoadedAt = DateTimeOffset.Now;
		}
		return _library;
	}

	private static async Task GameTickAsync()
	{
		if (_gameBusy)
		{
			return;
		}
		IGameBoostService boost = AppServices.GameBoost;
		BoostState state = boost.State;
		int? num = state?.AutoProcessId;
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			if (!RunningGames.IsRunning(valueOrDefault))
			{
				_gameBusy = true;
				try
				{
					await boost.StopAsync(reopenApps: true).ConfigureAwait(continueOnCapturedContext: true);
					AppServices.Logger.Info("Automatic Game Boost stopped: " + state.AutoGameName + " closed.");
					TrayIcon.Notify(AppServices.Localizer["autoBoost.notify.title"], AppServices.Localizer.Format("autoBoost.notify.stopped", state.AutoGameName));
					OnBoostChanged();
				}
				finally
				{
					_gameBusy = false;
				}
			}
		}
		else
		{
			if ((object)state != null || !AppServices.Settings.Current.AutoBoostEnabled)
			{
				return;
			}
			_gameBusy = true;
			try
			{
				IReadOnlyList<GameEntry> library = GetLibrary();
				IReadOnlyList<string> excluded = AppServices.Settings.Current.AutoBoostExcludedGames;
				RunningGame game = await Task.Run(() => RunningGames.FindFirst(library, excluded)).ConfigureAwait(continueOnCapturedContext: true);
				if ((object)game != null)
				{
					BoostReport boostReport = await boost.StartAsync(DefaultOptions(automatic: true)with
					{
						AutoGameName = game.Game.Name,
						AutoProcessId = game.ProcessId
					}).ConfigureAwait(continueOnCapturedContext: true);
					AppServices.Logger.Info($"Automatic Game Boost started for {game.Game.Name} ({game.ExecutablePath}).");
					ILocalizer localizer = AppServices.Localizer;
					TrayIcon.Notify(localizer["autoBoost.notify.title"], (boostReport.ClosedApps > 0) ? localizer.Format("autoBoost.notify.startedClosed", game.Game.Name, boostReport.ClosedApps) : localizer.Format("autoBoost.notify.started", game.Game.Name));
					OnBoostChanged();
				}
			}
			catch (Exception exception)
			{
				AppServices.Logger.Error("Automatic Game Boost failed", exception);
			}
			finally
			{
				_gameBusy = false;
			}
		}
	}

	private static BoostOptions DefaultOptions(bool automatic)
	{
		List<string> appsToClose = (from a in AppServices.GameBoost.GetClosableApps()
			where a.SelectedByDefault && (!automatic || !Launchers.Contains(a.ProcessName))
			select a.ProcessName).ToList();
		return new BoostOptions(SwitchPowerPlan: true, Elevation.IsElevated, !automatic, appsToClose);
	}

	public static async Task ToggleBoostFromTrayAsync()
	{
		IGameBoostService gameBoost = AppServices.GameBoost;
		ILocalizer loc = AppServices.Localizer;
		try
		{
			if (gameBoost.IsActive)
			{
				await gameBoost.StopAsync(reopenApps: true).ConfigureAwait(continueOnCapturedContext: true);
				TrayIcon.Notify(loc["boost.title"], loc["boost.stopped"]);
			}
			else
			{
				BoostReport boostReport = await gameBoost.StartAsync(DefaultOptions(automatic: false)).ConfigureAwait(continueOnCapturedContext: true);
				TrayIcon.Notify(loc["boost.title"], string.Join(" · ", new string[1] { loc["boost.started"] }.Concat(boostReport.Warnings)));
			}
		}
		catch (Exception exception)
		{
			AppServices.Logger.Error("Game Boost from tray failed", exception);
		}
		OnBoostChanged();
	}

	public static async Task RunMaintenanceFromTrayAsync()
	{
		if (_busy)
		{
			return;
		}
		_busy = true;
		try
		{
			MaintenanceReport maintenanceReport = await RunMaintenanceAsync().ConfigureAwait(continueOnCapturedContext: true);
			ILocalizer localizer = AppServices.Localizer;
			TrayIcon.Notify(localizer["maintenance.notify.title"], (maintenanceReport.DriftedTweaks > 0) ? localizer.Format("maintenance.notify.bodyDrift", GhostFormat.Bytes(maintenanceReport.FreedBytes), maintenanceReport.DriftedTweaks) : localizer.Format("maintenance.notify.body", GhostFormat.Bytes(maintenanceReport.FreedBytes)), (maintenanceReport.DriftedTweaks > 0) ? "dashboard" : null);
		}
		catch (Exception exception)
		{
			AppServices.Logger.Error("Maintenance from tray failed", exception);
		}
		finally
		{
			_busy = false;
		}
	}

	public static async Task FreeMemoryFromTrayAsync()
	{
		MemoryCleanResult memoryCleanResult = await AppServices.PerformanceTools.FreeMemoryAsync().ConfigureAwait(continueOnCapturedContext: true);
		TrayIcon.Notify(AppServices.Localizer["tray.freeMemory"], AppServices.Localizer.Format("tray.freeMemory.done", GhostFormat.Bytes(Math.Max(0L, memoryCleanResult.FreedBytes))));
	}

	private static void OnBoostChanged()
	{
		BoostState state = AppServices.GameBoost.State;
		object status;
		if ((object)state != null)
		{
			string autoGameName = state.AutoGameName;
			status = ((autoGameName != null) ? AppServices.Localizer.Format("tray.status.boostFor", autoGameName) : AppServices.Localizer["tray.status.boost"]);
		}
		else
		{
			status = null;
		}
		TrayIcon.SetStatus((string?)status);
		BoostChanged?.Invoke(null, EventArgs.Empty);
	}

	public static void RefreshStatus()
	{
		OnBoostChanged();
	}
}
