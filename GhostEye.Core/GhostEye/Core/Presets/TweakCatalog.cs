using System.Collections.Generic;
using System.Linq;
using GhostEye.Core.Models;

namespace GhostEye.Core.Presets;

public static class TweakCatalog
{
	private const string KeyPriorityControl = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl";

	private const string KeyVisualEffects = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects";

	private const string KeyDesktop = "HKCU\\Control Panel\\Desktop";

	private const string KeyGameBar = "HKCU\\Software\\Microsoft\\GameBar";

	private const string KeyGameDvr = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR";

	private const string KeyGameConfigStore = "HKCU\\System\\GameConfigStore";

	private const string KeyGraphicsDrivers = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers";

	private const string KeyMouse = "HKCU\\Control Panel\\Mouse";

	private const string KeyNetClass = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}";

	private const string KeyStartupApprovedUser = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved";

	private const string KeyStartupApprovedMachine = "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved";

	private const string KeyDataCollection = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection";

	private const string KeyAdvertisingInfo = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo";

	private const string KeySystemPolicies = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System";

	private const string KeyContentDelivery = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager";

	private const string KeySiuf = "HKCU\\Software\\Microsoft\\Siuf\\Rules";

	private const string KeyCopilotPolicyUser = "HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot";

	private const string KeyCopilotPolicyMachine = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsCopilot";

	private const string KeyExplorerAdvanced = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

	private const string KeyWindowsAiUser = "HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsAI";

	private const string KeyWindowsAiMachine = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI";

	private const string KeyDsh = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Dsh";

	private const string KeyExplorerPolicyUser = "HKCU\\Software\\Policies\\Microsoft\\Windows\\Explorer";

	private const string KeySearch = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Search";

	private const string KeyEdgePolicy = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge";

	private const string KeyInputPersonalization = "HKCU\\Software\\Microsoft\\InputPersonalization";

	private const string KeyInputTipc = "HKCU\\Software\\Microsoft\\Input\\TIPC";

	private const string KeyPrivacy = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Privacy";

	private const string KeyLocationConsent = "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CapabilityAccessManager\\ConsentStore\\location";

	private const string KeyClassicMenu = "HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";

	private const string KeyTaskbarDeveloper = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\\TaskbarDeveloperSettings";

	private const string KeyStickyKeys = "HKCU\\Control Panel\\Accessibility\\StickyKeys";

	private const string KeyFilterKeys = "HKCU\\Control Panel\\Accessibility\\Keyboard Response";

	private const string KeyToggleKeys = "HKCU\\Control Panel\\Accessibility\\ToggleKeys";

	private const string KeySystemProfile = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile";

	private const string KeyTcpInterfaces = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces";

	private const string KeyDeliveryOptimization = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeliveryOptimization";

	public const string PerfPowerPlanHigh = "perf.power-plan-high";

	public const string PerfForegroundPriority = "perf.foreground-priority";

	public const string PerfVisualEffects = "perf.visual-effects";

	public const string PerfStartupCleanup = "perf.startup-cleanup";

	public const string PerfMemoryCompression = "perf.memory-compression";

	public const string PerfStorageOptimize = "perf.storage-optimize";

	public const string PerfUltimatePlan = "perf.ultimate-plan";

	public const string GamingGameMode = "gaming.game-mode";

	public const string GamingDisableGameBar = "gaming.disable-game-bar";

	public const string GamingGpuScheduling = "gaming.gpu-scheduling";

	public const string GamingFullscreenOpt = "gaming.fullscreen-opt";

	public const string GamingBlockBackgroundRecording = "gaming.block-background-recording";

	public const string GamingMouseLatency = "gaming.mouse-latency";

	public const string NetworkDnsOptimized = "network.dns-optimized";

	public const string NetworkTcpOptimize = "network.tcp-optimize";

	public const string NetworkAdapterPowerOff = "network.adapter-power-off";

	public const string NetworkDnsOverHttps = "network.dns-doh";

	public const string NetworkGamingLatency = "network.gaming-latency";

	public const string NetworkDeliveryOptimization = "network.delivery-optimization";

	public const string PrivacyTelemetry = "privacy.telemetry";

	public const string PrivacyAdvertisingId = "privacy.advertising-id";

	public const string PrivacyActivityHistory = "privacy.activity-history";

	public const string PrivacyWindowsTips = "privacy.windows-tips";

	public const string PrivacyAutoFeedback = "privacy.auto-feedback";

	public const string PrivacyCopilot = "privacy.copilot";

	public const string PrivacyRecall = "privacy.recall";

	public const string PrivacyWidgets = "privacy.widgets";

	public const string PrivacyStartWebSearch = "privacy.start-web-search";

	public const string PrivacyStartAds = "privacy.start-ads";

	public const string PrivacyEdge = "privacy.edge";

	public const string PrivacyInkingTyping = "privacy.inking-typing";

	public const string PrivacyTailoredExperiences = "privacy.tailored-experiences";

	public const string PrivacyLocation = "privacy.location";

	public const string UiClassicContextMenu = "ui.classic-context-menu";

	public const string UiFileExtensions = "ui.file-extensions";

	public const string UiEndTask = "ui.end-task";

	public const string UiExplorerThisPc = "ui.explorer-this-pc";

	public const string UiTaskbarLeft = "ui.taskbar-left";

	public const string UiStickyKeysShortcut = "ui.sticky-keys-shortcut";

	private static readonly IReadOnlyList<TweakDefinition> AllTweaks;

	public static IReadOnlyList<TweakDefinition> All => AllTweaks;

	private static TweakDefinition Define(string id, TweakCategory category, RiskLevel risk, params string[] backupKeys)
	{
		return new TweakDefinition
		{
			Id = id,
			NameKey = "tweak." + id + ".name",
			DescriptionKey = "tweak." + id + ".desc",
			Category = category,
			Risk = risk,
			BackupKeys = backupKeys
		};
	}

	public static IReadOnlyList<TweakDefinition> ByCategory(TweakCategory category)
	{
		return AllTweaks.Where((TweakDefinition t) => t.Category == category).ToList();
	}

	public static TweakDefinition? Find(string id)
	{
		return AllTweaks.FirstOrDefault((TweakDefinition t) => t.Id == id);
	}

	public static TweakDefinition Get(string id)
	{
		return Find(id) ?? throw new KeyNotFoundException("Tweak '" + id + "' is not in the catalog.");
	}

	static TweakCatalog()
	{
		List<TweakDefinition> list = new List<TweakDefinition>
		{
			new TweakDefinition
			{
				Id = "perf.power-plan-high",
				NameKey = "tweak.perf.power-plan-high.name",
				DescriptionKey = "tweak.perf.power-plan-high.desc",
				Category = TweakCategory.Performance,
				Risk = RiskLevel.Low
			},
			new TweakDefinition
			{
				Id = "perf.foreground-priority",
				NameKey = "tweak.perf.foreground-priority.name",
				DescriptionKey = "tweak.perf.foreground-priority.desc",
				Category = TweakCategory.Performance,
				Risk = RiskLevel.Low,
				BackupKeys = new string[1] { "HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl" }
			},
			new TweakDefinition
			{
				Id = "perf.visual-effects",
				NameKey = "tweak.perf.visual-effects.name",
				DescriptionKey = "tweak.perf.visual-effects.desc",
				Category = TweakCategory.Performance,
				Risk = RiskLevel.Medium,
				BackupKeys = new string[2] { "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects", "HKCU\\Control Panel\\Desktop" }
			},
			new TweakDefinition
			{
				Id = "perf.startup-cleanup",
				NameKey = "tweak.perf.startup-cleanup.name",
				DescriptionKey = "tweak.perf.startup-cleanup.desc",
				Category = TweakCategory.Performance,
				Risk = RiskLevel.Low,
				BackupKeys = new string[2] { "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved", "HKLM\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StartupApproved" }
			},
			new TweakDefinition
			{
				Id = "perf.memory-compression",
				NameKey = "tweak.perf.memory-compression.name",
				DescriptionKey = "tweak.perf.memory-compression.desc",
				Category = TweakCategory.Performance,
				Risk = RiskLevel.Medium,
				AdvancedOnly = true
			},
			new TweakDefinition
			{
				Id = "perf.storage-optimize",
				NameKey = "tweak.perf.storage-optimize.name",
				DescriptionKey = "tweak.perf.storage-optimize.desc",
				Category = TweakCategory.Performance,
				Risk = RiskLevel.Low
			},
			new TweakDefinition
			{
				Id = "perf.ultimate-plan",
				NameKey = "tweak.perf.ultimate-plan.name",
				DescriptionKey = "tweak.perf.ultimate-plan.desc",
				Category = TweakCategory.Performance,
				Risk = RiskLevel.Medium,
				AdvancedOnly = true
			},
			new TweakDefinition
			{
				Id = "gaming.game-mode",
				NameKey = "tweak.gaming.game-mode.name",
				DescriptionKey = "tweak.gaming.game-mode.desc",
				Category = TweakCategory.Gaming,
				Risk = RiskLevel.Low,
				BackupKeys = new string[1] { "HKCU\\Software\\Microsoft\\GameBar" }
			},
			new TweakDefinition
			{
				Id = "gaming.disable-game-bar",
				NameKey = "tweak.gaming.disable-game-bar.name",
				DescriptionKey = "tweak.gaming.disable-game-bar.desc",
				Category = TweakCategory.Gaming,
				Risk = RiskLevel.Low,
				BackupKeys = new string[2] { "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR", "HKCU\\System\\GameConfigStore" }
			},
			new TweakDefinition
			{
				Id = "gaming.gpu-scheduling",
				NameKey = "tweak.gaming.gpu-scheduling.name",
				DescriptionKey = "tweak.gaming.gpu-scheduling.desc",
				Category = TweakCategory.Gaming,
				Risk = RiskLevel.Medium,
				RequiresRestart = true,
				BackupKeys = new string[1] { "HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers" }
			},
			new TweakDefinition
			{
				Id = "gaming.fullscreen-opt",
				NameKey = "tweak.gaming.fullscreen-opt.name",
				DescriptionKey = "tweak.gaming.fullscreen-opt.desc",
				Category = TweakCategory.Gaming,
				Risk = RiskLevel.Low,
				BackupKeys = new string[1] { "HKCU\\System\\GameConfigStore" }
			},
			new TweakDefinition
			{
				Id = "gaming.block-background-recording",
				NameKey = "tweak.gaming.block-background-recording.name",
				DescriptionKey = "tweak.gaming.block-background-recording.desc",
				Category = TweakCategory.Gaming,
				Risk = RiskLevel.Low,
				BackupKeys = new string[1] { "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR" }
			},
			new TweakDefinition
			{
				Id = "gaming.mouse-latency",
				NameKey = "tweak.gaming.mouse-latency.name",
				DescriptionKey = "tweak.gaming.mouse-latency.desc",
				Category = TweakCategory.Gaming,
				Risk = RiskLevel.Medium,
				BackupKeys = new string[1] { "HKCU\\Control Panel\\Mouse" }
			},
			new TweakDefinition
			{
				Id = "network.dns-optimized",
				NameKey = "tweak.network.dns-optimized.name",
				DescriptionKey = "tweak.network.dns-optimized.desc",
				Category = TweakCategory.Network,
				Risk = RiskLevel.Medium
			},
			new TweakDefinition
			{
				Id = "network.tcp-optimize",
				NameKey = "tweak.network.tcp-optimize.name",
				DescriptionKey = "tweak.network.tcp-optimize.desc",
				Category = TweakCategory.Network,
				Risk = RiskLevel.Low
			},
			new TweakDefinition
			{
				Id = "network.adapter-power-off",
				NameKey = "tweak.network.adapter-power-off.name",
				DescriptionKey = "tweak.network.adapter-power-off.desc",
				Category = TweakCategory.Network,
				Risk = RiskLevel.Low,
				BackupKeys = new string[1] { "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}" }
			},
			Define("network.dns-doh", TweakCategory.Network, RiskLevel.Medium)
		};
		string[] backupKeys = new string[2] { "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces" };
		list.Add(Define("network.gaming-latency", TweakCategory.Network, RiskLevel.Low, backupKeys)with
		{
			RequiresRestart = true
		});
		list.Add(Define("network.delivery-optimization", TweakCategory.Network, RiskLevel.Low, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeliveryOptimization"));
		list.Add(new TweakDefinition
		{
			Id = "privacy.telemetry",
			NameKey = "tweak.privacy.telemetry.name",
			DescriptionKey = "tweak.privacy.telemetry.desc",
			Category = TweakCategory.Privacy,
			Risk = RiskLevel.Low,
			BackupKeys = new string[1] { "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection" }
		});
		list.Add(new TweakDefinition
		{
			Id = "privacy.advertising-id",
			NameKey = "tweak.privacy.advertising-id.name",
			DescriptionKey = "tweak.privacy.advertising-id.desc",
			Category = TweakCategory.Privacy,
			Risk = RiskLevel.Low,
			BackupKeys = new string[1] { "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo" }
		});
		list.Add(new TweakDefinition
		{
			Id = "privacy.activity-history",
			NameKey = "tweak.privacy.activity-history.name",
			DescriptionKey = "tweak.privacy.activity-history.desc",
			Category = TweakCategory.Privacy,
			Risk = RiskLevel.Medium,
			BackupKeys = new string[1] { "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System" }
		});
		list.Add(new TweakDefinition
		{
			Id = "privacy.windows-tips",
			NameKey = "tweak.privacy.windows-tips.name",
			DescriptionKey = "tweak.privacy.windows-tips.desc",
			Category = TweakCategory.Privacy,
			Risk = RiskLevel.Low,
			BackupKeys = new string[1] { "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager" }
		});
		list.Add(new TweakDefinition
		{
			Id = "privacy.auto-feedback",
			NameKey = "tweak.privacy.auto-feedback.name",
			DescriptionKey = "tweak.privacy.auto-feedback.desc",
			Category = TweakCategory.Privacy,
			Risk = RiskLevel.Low,
			BackupKeys = new string[1] { "HKCU\\Software\\Microsoft\\Siuf\\Rules" }
		});
		list.Add(Define("privacy.copilot", TweakCategory.Privacy, RiskLevel.Low, "HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot", "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsCopilot", "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced"));
		list.Add(Define("privacy.recall", TweakCategory.Privacy, RiskLevel.Low, "HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsAI", "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI"));
		list.Add(Define("privacy.widgets", TweakCategory.Privacy, RiskLevel.Low, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Dsh"));
		list.Add(Define("privacy.start-web-search", TweakCategory.Privacy, RiskLevel.Low, "HKCU\\Software\\Policies\\Microsoft\\Windows\\Explorer", "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Search"));
		list.Add(Define("privacy.start-ads", TweakCategory.Privacy, RiskLevel.Low, "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager"));
		list.Add(Define("privacy.edge", TweakCategory.Privacy, RiskLevel.Medium, "HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge"));
		list.Add(Define("privacy.inking-typing", TweakCategory.Privacy, RiskLevel.Low, "HKCU\\Software\\Microsoft\\InputPersonalization", "HKCU\\Software\\Microsoft\\Input\\TIPC"));
		list.Add(Define("privacy.tailored-experiences", TweakCategory.Privacy, RiskLevel.Low, "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Privacy"));
		list.Add(Define("privacy.location", TweakCategory.Privacy, RiskLevel.Medium, "HKLM\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CapabilityAccessManager\\ConsentStore\\location"));
		string[] backupKeys2 = new string[1] { "HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}" };
		list.Add(Define("ui.classic-context-menu", TweakCategory.Interface, RiskLevel.Low, backupKeys2)with
		{
			RequiresRestart = true
		});
		list.Add(Define("ui.file-extensions", TweakCategory.Interface, RiskLevel.Low, "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced"));
		list.Add(Define("ui.end-task", TweakCategory.Interface, RiskLevel.Low, "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\\TaskbarDeveloperSettings"));
		list.Add(Define("ui.explorer-this-pc", TweakCategory.Interface, RiskLevel.Low, "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced"));
		list.Add(Define("ui.taskbar-left", TweakCategory.Interface, RiskLevel.Low, "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced"));
		list.Add(Define("ui.sticky-keys-shortcut", TweakCategory.Interface, RiskLevel.Low, "HKCU\\Control Panel\\Accessibility\\StickyKeys", "HKCU\\Control Panel\\Accessibility\\Keyboard Response", "HKCU\\Control Panel\\Accessibility\\ToggleKeys"));
		AllTweaks = list;
	}
}
