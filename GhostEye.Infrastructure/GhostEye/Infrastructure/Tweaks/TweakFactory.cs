using System.Collections.Generic;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public static class TweakFactory
{
	public static IReadOnlyList<ITweakAction> CreateAll(IRegistryService registry, IPowerPlanService powerPlans, IStartupManagerService startup, IProcessRunner processRunner, IDnsPreferenceProvider dnsPreferences, ILocalizer loc)
	{
		return new _003C_003Ez__ReadOnlyArray<ITweakAction>(new ITweakAction[39]
		{
			new PowerPlanHighTweak(powerPlans, loc),
			new ForegroundPriorityTweak(registry, loc),
			new VisualEffectsTweak(registry, loc),
			new StartupCleanupTweak(startup, loc),
			new MemoryCompressionTweak(processRunner, loc),
			new StorageOptimizeTweak(processRunner, loc),
			new UltimatePlanTweak(powerPlans, processRunner, registry, loc),
			new GameModeTweak(registry, loc),
			new DisableGameBarTweak(registry, loc),
			new GpuSchedulingTweak(registry, loc),
			new FullscreenOptimizationsTweak(registry, loc),
			new BlockBackgroundRecordingTweak(registry, loc),
			new MouseLatencyTweak(registry, loc),
			new DnsTweak(dnsPreferences, processRunner, loc),
			new TcpAutotuningTweak(processRunner, loc),
			new NetworkAdapterPowerTweak(registry, loc),
			new DnsOverHttpsTweak(dnsPreferences, processRunner, loc),
			new GamingLatencyTweak(registry, loc),
			new DeliveryOptimizationTweak(registry, loc),
			new TelemetryTweak(registry, loc),
			new AdvertisingIdTweak(registry, loc),
			new ActivityHistoryTweak(registry, loc),
			new WindowsTipsTweak(registry, loc),
			new AutoFeedbackTweak(registry, loc),
			new CopilotTweak(registry, loc),
			new RecallTweak(registry, loc),
			new WidgetsTweak(registry, loc),
			new StartWebSearchTweak(registry, loc),
			new StartAdsTweak(registry, loc),
			new EdgeTweak(registry, loc),
			new InkingTypingTweak(registry, loc),
			new TailoredExperiencesTweak(registry, loc),
			new LocationTweak(registry, loc),
			new ClassicContextMenuTweak(registry, loc),
			new FileExtensionsTweak(registry, loc),
			new EndTaskTweak(registry, loc),
			new ExplorerThisPcTweak(registry, loc),
			new TaskbarLeftTweak(registry, loc),
			new StickyKeysShortcutTweak(registry, loc)
		});
	}
}
