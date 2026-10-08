using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class StartAdsTweak : RegistryTweakBase
{
	private const string Advanced = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

	private const string ContentDelivery = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[5]
	{
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "Start_IrisRecommendations", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "Start_AccountNotifications", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0)
	});

	public StartAdsTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.start-ads", registry, loc)
	{
	}

	protected override void AfterApply()
	{
		NativeUi.RefreshShell();
	}
}
