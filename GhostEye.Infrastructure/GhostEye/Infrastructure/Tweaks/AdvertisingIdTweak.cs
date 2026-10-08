using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class AdvertisingIdTweak : RegistryTweakBase
{
	private const string AdvertisingInfo = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\AdvertisingInfo", "Enabled", 0));

	protected override string SuccessKey => "result.advertisingId";

	public AdvertisingIdTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.advertising-id", registry, loc)
	{
	}
}
