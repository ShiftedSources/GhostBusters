using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class DisableGameBarTweak : RegistryTweakBase
{
	private const string GameDvr = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR";

	private const string GameConfigStore = "HKCU\\System\\GameConfigStore";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[2]
	{
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR", "AppCaptureEnabled", 0),
		RegistryWrite.Dword("HKCU\\System\\GameConfigStore", "GameDVR_Enabled", 0)
	});

	protected override string SuccessKey => "result.gameBar";

	public DisableGameBarTweak(IRegistryService registry, ILocalizer loc)
		: base("gaming.disable-game-bar", registry, loc)
	{
	}
}
