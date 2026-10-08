using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class ForegroundPriorityTweak : RegistryTweakBase
{
	private const string Key = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKLM\\SYSTEM\\CurrentControlSet\\Control\\PriorityControl", "Win32PrioritySeparation", 38));

	protected override string SuccessKey => "result.foregroundPriority";

	public ForegroundPriorityTweak(IRegistryService registry, ILocalizer loc)
		: base("perf.foreground-priority", registry, loc)
	{
	}
}
