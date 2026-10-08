using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class RecallTweak : RegistryTweakBase
{
	private const string PolicyUser = "HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsAI";

	private const string PolicyMachine = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[3]
	{
		RegistryWrite.Dword("HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsAI", "DisableAIDataAnalysis", 1),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsAI", "DisableAIDataAnalysis", 1),
		RegistryWrite.Dword("HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsAI", "DisableClickToDo", 1)
	});

	public RecallTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.recall", registry, loc)
	{
	}
}
