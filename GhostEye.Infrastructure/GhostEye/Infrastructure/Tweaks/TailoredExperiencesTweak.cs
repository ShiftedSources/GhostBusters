using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class TailoredExperiencesTweak : RegistryTweakBase
{
	private const string Privacy = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Privacy";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0));

	public TailoredExperiencesTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.tailored-experiences", registry, loc)
	{
	}
}
