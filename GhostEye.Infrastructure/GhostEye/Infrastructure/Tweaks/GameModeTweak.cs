using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class GameModeTweak : RegistryTweakBase
{
	private const string GameBar = "HKCU\\Software\\Microsoft\\GameBar";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[2]
	{
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\GameBar", "AutoGameModeEnabled", 1),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\GameBar", "AllowAutoGameMode", 1)
	});

	protected override string SuccessKey => "result.gameMode";

	public GameModeTweak(IRegistryService registry, ILocalizer loc)
		: base("gaming.game-mode", registry, loc)
	{
	}
}
