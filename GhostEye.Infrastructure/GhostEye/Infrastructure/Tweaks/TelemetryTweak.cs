using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class TelemetryTweak : RegistryTweakBase
{
	private const string DataCollection = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[2]
	{
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection", "AllowTelemetry", 0),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection", "DoNotShowFeedbackNotifications", 1)
	});

	protected override string SuccessKey => "result.telemetry";

	public TelemetryTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.telemetry", registry, loc)
	{
	}
}
