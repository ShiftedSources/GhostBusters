using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class ExplorerThisPcTweak : RegistryTweakBase
{
	private const string Advanced = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "LaunchTo", 1));

	public ExplorerThisPcTweak(IRegistryService registry, ILocalizer loc)
		: base("ui.explorer-this-pc", registry, loc)
	{
	}
}
