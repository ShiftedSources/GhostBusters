using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class EdgeTweak : RegistryTweakBase
{
	private const string Edge = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[6]
	{
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge", "StartupBoostEnabled", 0),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge", "BackgroundModeEnabled", 0),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge", "HubsSidebarEnabled", 0),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge", "PersonalizationReportingEnabled", 0),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge", "ShowRecommendationsEnabled", 0),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Edge", "DiagnosticData", 0)
	});

	public EdgeTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.edge", registry, loc)
	{
	}
}
