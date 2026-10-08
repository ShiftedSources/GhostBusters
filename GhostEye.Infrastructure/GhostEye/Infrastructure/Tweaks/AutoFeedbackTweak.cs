using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class AutoFeedbackTweak : RegistryTweakBase
{
	private const string Siuf = "HKCU\\Software\\Microsoft\\Siuf\\Rules";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[2]
	{
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Siuf\\Rules", "NumberOfSIUFInPeriod", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Siuf\\Rules", "PeriodInNanoSeconds", 0)
	});

	protected override string SuccessKey => "result.autoFeedback";

	public AutoFeedbackTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.auto-feedback", registry, loc)
	{
	}
}
