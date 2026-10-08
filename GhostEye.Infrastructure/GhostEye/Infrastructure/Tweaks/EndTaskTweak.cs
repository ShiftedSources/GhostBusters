using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class EndTaskTweak : RegistryTweakBase
{
	private const string DeveloperSettings = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\\TaskbarDeveloperSettings";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\\TaskbarDeveloperSettings", "TaskbarEndTask", 1));

	public EndTaskTweak(IRegistryService registry, ILocalizer loc)
		: base("ui.end-task", registry, loc)
	{
	}

	protected override void AfterApply()
	{
		NativeUi.RefreshShell();
	}
}
