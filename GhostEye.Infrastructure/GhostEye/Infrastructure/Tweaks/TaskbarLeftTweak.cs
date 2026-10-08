using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class TaskbarLeftTweak : RegistryTweakBase
{
	private const string Advanced = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "TaskbarAl", 0));

	public TaskbarLeftTweak(IRegistryService registry, ILocalizer loc)
		: base("ui.taskbar-left", registry, loc)
	{
	}

	protected override void AfterApply()
	{
		NativeUi.RefreshShell();
	}
}
