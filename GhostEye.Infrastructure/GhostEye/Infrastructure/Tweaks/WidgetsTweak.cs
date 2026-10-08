using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class WidgetsTweak : RegistryTweakBase
{
	private const string Dsh = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Dsh";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Dsh", "AllowNewsAndInterests", 0));

	public WidgetsTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.widgets", registry, loc)
	{
	}

	protected override void AfterApply()
	{
		NativeUi.RefreshShell();
	}
}
