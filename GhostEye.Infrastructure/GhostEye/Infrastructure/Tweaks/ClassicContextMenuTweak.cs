using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class ClassicContextMenuTweak : RegistryTweakBase
{
	private const string Clsid = "HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";

	private const string InprocServer = "HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(new RegistryWrite("HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32", string.Empty, string.Empty, RegistryValueKind.String));

	protected override bool RequiresRestart => true;

	public ClassicContextMenuTweak(IRegistryService registry, ILocalizer loc)
		: base("ui.classic-context-menu", registry, loc)
	{
	}

	protected override void AfterApply()
	{
	}

	protected override void AfterRevert()
	{
		if (Registry.ReadValue("HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32", string.Empty) == null)
		{
			Registry.DeleteKey("HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");
		}
	}
}
