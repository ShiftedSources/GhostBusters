using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class MouseLatencyTweak : RegistryTweakBase
{
	private const string Mouse = "HKCU\\Control Panel\\Mouse";

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[3]
	{
		RegistryWrite.Text("HKCU\\Control Panel\\Mouse", "MouseSpeed", "0"),
		RegistryWrite.Text("HKCU\\Control Panel\\Mouse", "MouseThreshold1", "0"),
		RegistryWrite.Text("HKCU\\Control Panel\\Mouse", "MouseThreshold2", "0")
	});

	protected override string SuccessKey => "result.mouseLatency";

	public MouseLatencyTweak(IRegistryService registry, ILocalizer loc)
		: base("gaming.mouse-latency", registry, loc)
	{
	}

	protected override void AfterApply()
	{
		NativeUi.ApplyMouseParameters(0, 0, 0);
	}

	protected override void AfterRevert()
	{
		int threshold = ReadInt("MouseThreshold1", 6);
		int threshold2 = ReadInt("MouseThreshold2", 10);
		int speed = ReadInt("MouseSpeed", 1);
		NativeUi.ApplyMouseParameters(threshold, threshold2, speed);
	}

	private int ReadInt(string name, int fallback)
	{
		if (!int.TryParse(Registry.ReadString("HKCU\\Control Panel\\Mouse", name), out var result))
		{
			return fallback;
		}
		return result;
	}
}
