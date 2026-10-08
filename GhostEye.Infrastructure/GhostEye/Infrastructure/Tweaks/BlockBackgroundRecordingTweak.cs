using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class BlockBackgroundRecordingTweak : RegistryTweakBase
{
	private const string GameDvr = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlySingleElementList<RegistryWrite>(RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\GameDVR", "HistoricalCaptureEnabled", 0));

	protected override string SuccessKey => "result.backgroundRecording";

	public BlockBackgroundRecordingTweak(IRegistryService registry, ILocalizer loc)
		: base("gaming.block-background-recording", registry, loc)
	{
	}
}
