using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class ActivityHistoryTweak : RegistryTweakBase
{
	private const string SystemPolicies = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[2]
	{
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System", "PublishUserActivities", 0),
		RegistryWrite.Dword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System", "UploadUserActivities", 0)
	});

	protected override string SuccessKey => "result.activityHistory";

	public ActivityHistoryTweak(IRegistryService registry, ILocalizer loc)
		: base("privacy.activity-history", registry, loc)
	{
	}
}
