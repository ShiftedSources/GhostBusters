using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class CopilotTweak : RegistryTweakBase
{
	private const string PolicyUser = "HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot";

	private const string PolicyMachine = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsCopilot";

	private const string Advanced = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[3]
	{
		RegistryWrite.Dword("HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot", "TurnOffWindowsCopilot", 1),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsCopilot", "TurnOffWindowsCopilot", 1),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ShowCopilotButton", 0)
	});

	public CopilotTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.copilot", registry, loc)
	{
	}

	protected override void AfterApply()
	{
		NativeUi.RefreshShell();
	}
}
