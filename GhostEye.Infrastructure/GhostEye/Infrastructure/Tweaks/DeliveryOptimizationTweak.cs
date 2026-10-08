using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class DeliveryOptimizationTweak : RegistryTweakBase
{
	private const string Policy = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeliveryOptimization";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DeliveryOptimization", "DODownloadMode", 0));

	protected override string SuccessKey => "result.deliveryOptimization";

	public DeliveryOptimizationTweak(IRegistryService registry, ILocalizer loc)
		: base("network.delivery-optimization", registry, loc)
	{
	}
}
