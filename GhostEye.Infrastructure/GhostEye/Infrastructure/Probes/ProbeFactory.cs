using System.Collections.Generic;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;
using GhostEye.Infrastructure.Tweaks;

namespace GhostEye.Infrastructure.Probes;

public static class ProbeFactory
{
	public static IReadOnlyList<IAnalysisProbe> CreateAll(IRegistryService registry, IPowerPlanService powerPlans, IServiceManagerService services, IStartupManagerService startup, IProcessRunner processRunner, ILocalizer loc)
	{
		return new _003C_003Ez__ReadOnlyArray<IAnalysisProbe>(new IAnalysisProbe[13]
		{
			new CpuProbe(loc),
			new GpuProbe(registry, loc),
			new RamProbe(loc),
			new StorageProbe(processRunner, loc),
			new PowerPlanProbe(powerPlans, loc),
			new StartupAppsProbe(startup, loc),
			new BackgroundAppsProbe(registry, loc),
			new ServicesProbe(services, loc),
			new WindowsUpdateProbe(registry, loc),
			new DnsProbe(loc),
			new TcpProbe(new TcpAutotuningTweak(processRunner, loc), loc),
			new AdapterPowerProbe(registry, loc),
			new NicBufferProbe(registry, loc)
		});
	}
}
