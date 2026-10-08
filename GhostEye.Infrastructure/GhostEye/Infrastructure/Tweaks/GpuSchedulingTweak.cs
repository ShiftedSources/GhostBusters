using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class GpuSchedulingTweak : RegistryTweakBase
{
	private const string GraphicsDrivers = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", "HwSchMode", 2));

	protected override bool RequiresRestart => true;

	protected override string SuccessKey => "result.gpuScheduling";

	public GpuSchedulingTweak(IRegistryService registry, ILocalizer loc)
		: base("gaming.gpu-scheduling", registry, loc)
	{
	}

	public override Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(Registry.ReadDword("HKLM\\SYSTEM\\CurrentControlSet\\Control\\GraphicsDrivers", "HwSchMode").HasValue ? TweakApplicability.Applicable : TweakApplicability.No(Loc["applicability.gpuSchedulingUnsupported"]));
	}
}
