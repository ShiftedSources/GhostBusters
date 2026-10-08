using System;
using System.Runtime.InteropServices;

namespace GhostEye.Infrastructure.Tweaks;

internal static class NativeUi
{
	private struct FlagsOnly
	{
		public uint cbSize;

		public uint dwFlags;
	}

	private struct FilterKeys
	{
		public uint cbSize;

		public uint dwFlags;

		public uint iWaitMSec;

		public uint iDelayMSec;

		public uint iRepeatMSec;

		public uint iBounceMSec;
	}

	private const int HwndBroadcast = 65535;

	private const int WmSettingChange = 26;

	private const int SmtoAbortIfHung = 2;

	private const uint SpiSetMouse = 4u;

	private const uint SpiSetDragFullWindows = 37u;

	private const uint SpiSetUiEffects = 4159u;

	private const uint SpifUpdateIniFile = 1u;

	private const uint SpifSendChange = 2u;

	private const int ShcneAssocChanged = 134217728;

	private const uint SpiGetFilterKeys = 50u;

	private const uint SpiSetFilterKeys = 51u;

	private const uint SpiGetToggleKeys = 52u;

	private const uint SpiSetToggleKeys = 53u;

	private const uint SpiGetStickyKeys = 58u;

	private const uint SpiSetStickyKeys = 59u;

	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern nint SendMessageTimeout(nint hWnd, int msg, nint wParam, string lParam, int flags, int timeoutMilliseconds, out nint result);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SystemParametersInfo(uint action, uint param, int[] value, uint winIni);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SystemParametersInfo(uint action, uint param, nint value, uint winIni);

	public static void BroadcastSettingChange(string section)
	{
		SendMessageTimeout(65535, 26, IntPtr.Zero, section, 2, 1000, out var _);
	}

	[DllImport("shell32.dll")]
	private static extern void SHChangeNotify(int eventId, uint flags, nint item1, nint item2);

	public static void RefreshShell()
	{
		SHChangeNotify(134217728, 0u, IntPtr.Zero, IntPtr.Zero);
		BroadcastSettingChange("TraySettings");
	}

	public static void ApplyMouseParameters(int threshold1, int threshold2, int speed)
	{
		SystemParametersInfo(4u, 0u, new int[3] { threshold1, threshold2, speed }, 3u);
	}

	public static void SetDragFullWindows(bool enabled)
	{
		SystemParametersInfo(37u, enabled ? 1u : 0u, IntPtr.Zero, 3u);
	}

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SystemParametersInfo(uint action, uint param, ref FlagsOnly value, uint winIni);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool SystemParametersInfo(uint action, uint param, ref FilterKeys value, uint winIni);

	public static void ApplyAccessibilityFlags(uint? stickyKeys, uint? filterKeys, uint? toggleKeys)
	{
		if (stickyKeys.HasValue)
		{
			uint valueOrDefault = stickyKeys.GetValueOrDefault();
			SetFlags(58u, 59u, valueOrDefault);
		}
		if (toggleKeys.HasValue)
		{
			uint valueOrDefault2 = toggleKeys.GetValueOrDefault();
			SetFlags(52u, 53u, valueOrDefault2);
		}
		if (filterKeys.HasValue)
		{
			uint valueOrDefault3 = filterKeys.GetValueOrDefault();
			FilterKeys value = new FilterKeys
			{
				cbSize = (uint)Marshal.SizeOf<FilterKeys>()
			};
			if (SystemParametersInfo(50u, value.cbSize, ref value, 0u))
			{
				value.dwFlags = valueOrDefault3;
				SystemParametersInfo(51u, value.cbSize, ref value, 3u);
			}
		}
		static void SetFlags(uint get, uint set, uint flags)
		{
			FlagsOnly value2 = new FlagsOnly
			{
				cbSize = (uint)Marshal.SizeOf<FlagsOnly>()
			};
			if (SystemParametersInfo(get, value2.cbSize, ref value2, 0u))
			{
				value2.dwFlags = flags;
				SystemParametersInfo(set, value2.cbSize, ref value2, 3u);
			}
		}
	}

	public static void SetUiEffects(bool enabled)
	{
		SystemParametersInfo(4159u, 0u, enabled ? new IntPtr(1) : IntPtr.Zero, 3u);
	}
}
