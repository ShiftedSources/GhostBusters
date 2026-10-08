using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class FullscreenOptimizationsTweak : RegistryTweakBase
{
	private const string GameConfigStore = "HKCU\\System\\GameConfigStore";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[4]
	{
		RegistryWrite.Dword("HKCU\\System\\GameConfigStore", "GameDVR_FSEBehaviorMode", 2),
		RegistryWrite.Dword("HKCU\\System\\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 1),
		RegistryWrite.Dword("HKCU\\System\\GameConfigStore", "GameDVR_DXGIHonorFSEWindowsCompatible", 1),
		RegistryWrite.Dword("HKCU\\System\\GameConfigStore", "GameDVR_EFSEFeatureFlags", 0)
	});

	protected override string SuccessKey => "result.fullscreenOpt";

	public FullscreenOptimizationsTweak(IRegistryService registry, ILocalizer loc)
		: base("gaming.fullscreen-opt", registry, loc)
	{
	}
}
