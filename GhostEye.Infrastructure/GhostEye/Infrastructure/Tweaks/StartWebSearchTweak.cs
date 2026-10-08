using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class StartWebSearchTweak : RegistryTweakBase
{
	private const string ExplorerPolicy = "HKCU\\Software\\Policies\\Microsoft\\Windows\\Explorer";

	private const string Search = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Search";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[2]
	{
		RegistryWrite.Dword("HKCU\\Software\\Policies\\Microsoft\\Windows\\Explorer", "DisableSearchBoxSuggestions", 1),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Search", "BingSearchEnabled", 0)
	});

	public StartWebSearchTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.start-web-search", registry, loc)
	{
	}

	protected override void AfterApply()
	{
		NativeUi.RefreshShell();
	}
}
