using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class WindowsTipsTweak : RegistryTweakBase
{
	private const string ContentDelivery = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[5]
	{
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SubscribedContent-338393Enabled", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SoftLandingEnabled", 0)
	});

	protected override string SuccessKey => "result.windowsTips";

	public WindowsTipsTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.windows-tips", registry, loc)
	{
	}
}
