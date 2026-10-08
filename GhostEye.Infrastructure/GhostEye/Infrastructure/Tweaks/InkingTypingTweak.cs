using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class InkingTypingTweak : RegistryTweakBase
{
	private const string InputPersonalization = "HKCU\\Software\\Microsoft\\InputPersonalization";

	private const string TrainedDataStore = "HKCU\\Software\\Microsoft\\InputPersonalization\\TrainedDataStore";

	private const string Tipc = "HKCU\\Software\\Microsoft\\Input\\TIPC";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[4]
	{
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\InputPersonalization", "RestrictImplicitInkCollection", 1),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\InputPersonalization", "RestrictImplicitTextCollection", 1),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\InputPersonalization\\TrainedDataStore", "HarvestContacts", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Input\\TIPC", "Enabled", 0)
	});

	public InkingTypingTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.inking-typing", registry, loc)
	{
	}
}
