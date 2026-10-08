using System;
using System.Collections.Generic;
using System.IO;
using GhostEye.App.Localization;
using GhostEye.App.ViewModels;
using GhostEye.Backup;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Services;
using GhostEye.Infrastructure.Apps;
using GhostEye.Infrastructure.Benchmarks;
using GhostEye.Infrastructure.Cleanup;
using GhostEye.Infrastructure.Disk;
using GhostEye.Infrastructure.Games;
using GhostEye.Infrastructure.Health;
using GhostEye.Infrastructure.Platform;
using GhostEye.Infrastructure.Probes;
using GhostEye.Infrastructure.Tweaks;

namespace GhostEye.App.Services;

public static class AppServices
{
	private static bool _initialized;

	public static ILocalizer Localizer { get; private set; } = null;

	public static IAppLogger Logger { get; private set; } = null;

	public static IRegistryService Registry { get; private set; } = null;

	public static IProcessRunner ProcessRunner { get; private set; } = null;

	public static IPowerPlanService PowerPlans { get; private set; } = null;

	public static IServiceManagerService ServiceManager { get; private set; } = null;

	public static IStartupManagerService StartupManager { get; private set; } = null;

	public static IRestorePointService RestorePoints { get; private set; } = null;

	public static IRegistryBackupService RegistryBackup { get; private set; } = null;

	public static IChangeHistoryStore History { get; private set; } = null;

	public static IOptimizationEngine Engine { get; private set; } = null;

	public static ISystemAnalyzer Analyzer { get; private set; } = null;

	public static ICleanupScanner Cleanup { get; private set; } = null;

	public static IInstalledAppsService InstalledApps { get; private set; } = null;

	public static ISystemOptimizerService SystemOptimizer { get; private set; } = null;

	public static IServiceBackupService ServiceBackups { get; private set; } = null;

	public static IPerformanceToolsService PerformanceTools { get; private set; } = null;

	public static IHealthService Health { get; private set; } = null;

	public static IDiskAnalyzer DiskAnalyzer { get; private set; } = null;

	public static INetworkToolsService NetworkTools { get; private set; } = null;

	public static IGameBoostService GameBoost { get; private set; } = null;

	public static ISnapshotService Snapshots { get; private set; } = null;

	public static AppSettingsService Settings { get; private set; } = null;

	public static DriftDetector Drift { get; private set; } = null;

	public static WingetService Winget { get; private set; } = null;

	public static GameLibrary Games { get; private set; } = null;

	public static FrameCaptureService FrameCapture { get; private set; } = null;

	public static FrameCaptureLog FrameCaptures { get; private set; } = null;

	public static ProfileService Profiles { get; private set; } = null;

	public static ShellViewModel Shell { get; set; } = null;

	public static DialogHostViewModel Dialogs => Shell.Dialogs;

	public static void Initialize()
	{
		if (!_initialized)
		{
			Logger = new FileLogger();
			Localizer = new Localizer(AppSettingsService.PeekLanguage());
			LocalizationSource.Instance.Refresh();
			Registry = new RegistryService();
			ProcessRunner = new ProcessRunner();
			PowerPlans = new PowerPlanService(Localizer);
			ServiceManager = new ServiceManagerService(Registry, Localizer);
			StartupManager = new StartupManagerService(Registry, ProcessRunner, Localizer, Path.Combine(AppPaths.Backups, "startup"));
			RestorePoints = new RestorePointService(Logger);
			RegistryBackup = new RegistryBackupService(ProcessRunner, Logger);
			History = new JsonChangeHistoryStore(Logger);
			Settings = new AppSettingsService(Logger);
			IReadOnlyList<ITweakAction> actions = TweakFactory.CreateAll(Registry, PowerPlans, StartupManager, ProcessRunner, Settings, Localizer);
			IReadOnlyList<IAnalysisProbe> probes = ProbeFactory.CreateAll(Registry, PowerPlans, ServiceManager, StartupManager, ProcessRunner, Localizer);
			Engine = new OptimizationEngine(actions, RestorePoints, RegistryBackup, History, Localizer, Logger);
			Analyzer = new SystemAnalyzer(probes, Localizer, Logger);
			Cleanup = new CleanupScanner(Logger, Localizer);
			InstalledApps = new InstalledAppsService(Registry, ProcessRunner, Localizer);
			SystemOptimizer = new SystemOptimizerService(ServiceManager, Registry, ProcessRunner, Localizer);
			ServiceBackups = new ServiceBackupService(ServiceManager, SystemOptimizer, ProcessRunner, Path.Combine(AppPaths.Backups, "services"));
			PerformanceTools = new PerformanceToolsService(ProcessRunner, Localizer);
			Health = new HealthService(ProcessRunner, Localizer);
			DiskAnalyzer = new DiskAnalyzer();
			NetworkTools = new NetworkToolsService(Localizer);
			GameBoost = new GameBoostService(PowerPlans, Registry, PerformanceTools, Localizer, Path.Combine(AppPaths.Root, "gameboost.json"));
			Snapshots = new SnapshotService(StartupManager, Logger, Localizer, Path.Combine(AppPaths.Root, "baseline.json"));
			Drift = new DriftDetector(History, Engine);
			Winget = new WingetService();
			Games = new GameLibrary(Registry);
			FrameCapture = new FrameCaptureService(FrameCaptureTool.Path, Localizer);
			FrameCaptures = new FrameCaptureLog(Path.Combine(AppPaths.Root, "fps.json"));
			Profiles = new ProfileService(Engine, SystemOptimizer, Settings, Localizer, Logger);
			_initialized = true;
			Logger.Info("GhostEye started, language = " + Localizer.Language + ".");
		}
	}

	public static void SetLanguage(string languageCode)
	{
		Localizer.SetLanguage(languageCode);
		LocalizationSource.Instance.Refresh();
		Shell?.OnLanguageChanged();
	}
}
