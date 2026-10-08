using System.Collections.Generic;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class VisualEffectsTweak : RegistryTweakBase
{
	private const string VisualFx = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects";

	private const string Desktop = "HKCU\\Control Panel\\Desktop";

	private const string WindowMetrics = "HKCU\\Control Panel\\Desktop\\WindowMetrics";

	private const string Advanced = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced";

	private const string Dwm = "HKCU\\Software\\Microsoft\\Windows\\DWM";

	private static readonly byte[] BestPerformanceMask = new byte[8] { 144, 18, 3, 128, 16, 0, 0, 0 };

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[8]
	{
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\VisualEffects", "VisualFXSetting", 2),
		new RegistryWrite("HKCU\\Control Panel\\Desktop", "UserPreferencesMask", BestPerformanceMask, RegistryValueKind.Binary),
		RegistryWrite.Text("HKCU\\Control Panel\\Desktop", "DragFullWindows", "0"),
		RegistryWrite.Text("HKCU\\Control Panel\\Desktop\\WindowMetrics", "MinAnimate", "0"),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ListviewAlphaSelect", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "ListviewShadow", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced", "TaskbarAnimations", 0),
		RegistryWrite.Dword("HKCU\\Software\\Microsoft\\Windows\\DWM", "EnableAeroPeek", 0)
	});

	protected override string SuccessKey => "result.visualEffects";

	public VisualEffectsTweak(IRegistryService registry, ILocalizer loc)
		: base("perf.visual-effects", registry, loc)
	{
	}

	protected override bool Matches(RegistryWrite write, object current)
	{
		if (write.Name != "UserPreferencesMask" || !(current is byte[] mask))
		{
			return base.Matches(write, current);
		}
		return IsMaskApplied(mask);
	}

	internal static bool IsMaskApplied(byte[] mask)
	{
		for (int i = 0; i < mask.Length; i++)
		{
			byte b = (byte)((i < BestPerformanceMask.Length) ? BestPerformanceMask[i] : 0);
			if ((mask[i] & ~b) != 0)
			{
				return false;
			}
		}
		return true;
	}

	protected override void AfterApply()
	{
		NativeUi.SetDragFullWindows(enabled: false);
		NativeUi.SetUiEffects(enabled: false);
		NativeUi.BroadcastSettingChange("WindowMetrics");
	}

	protected override void AfterRevert()
	{
		NativeUi.BroadcastSettingChange("WindowMetrics");
	}
}
