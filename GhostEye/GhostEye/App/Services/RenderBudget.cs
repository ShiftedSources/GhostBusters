using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace GhostEye.App.Services;

public static class RenderBudget
{
	private struct SystemPowerStatus
	{
		public byte ACLineStatus;

		public byte BatteryFlag;

		public byte BatteryLifePercent;

		public byte SystemStatusFlag;

		public int BatteryLifeTime;

		public int BatteryFullLifeTime;
	}

	private struct MemoryStatusEx
	{
		public uint dwLength;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;
	}

	private const long LowMemoryBytes = 4563402752L;

	private static bool? _isLowEnd;

	private static bool _watching;

	public static bool IsLowEnd
	{
		get
		{
			bool valueOrDefault = _isLowEnd == true;
			if (!_isLowEnd.HasValue)
			{
				valueOrDefault = DetectLowEnd();
				_isLowEnd = valueOrDefault;
				return valueOrDefault;
			}
			return valueOrDefault;
		}
	}

	public static bool OnBattery { get; private set; } = ReadOnBattery();

	public static bool AllowsAnimations
	{
		get
		{
			Watch();
			if (!IsLowEnd)
			{
				return !OnBattery;
			}
			return false;
		}
	}

	public static event EventHandler? Changed;

	private static void Watch()
	{
		if (_watching)
		{
			return;
		}
		_watching = true;
		SystemEvents.PowerModeChanged += (object _, PowerModeChangedEventArgs e) =>
		{
			if (e.Mode == PowerModes.StatusChange)
			{
				bool flag = ReadOnBattery();
				if (flag != OnBattery)
				{
					OnBattery = flag;
					Application.Current?.Dispatcher.BeginInvoke((Action)(() =>
					{
						Changed?.Invoke(null, EventArgs.Empty);
					}));
				}
			}
		};
	}

	private static bool DetectLowEnd()
	{
		int num = RenderCapability.Tier >> 16;
		MemoryStatusEx buffer = new MemoryStatusEx
		{
			dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>()
		};
		long num2 = (long)(GlobalMemoryStatusEx(ref buffer) ? buffer.ullTotalPhys : long.MaxValue);
		bool flag = num < 2 || SystemParameters.IsRemoteSession || Environment.ProcessorCount <= 4 || num2 <= 4563402752L;
		AppServices.Logger.Info($"Render budget: tier {num}, remote = {SystemParameters.IsRemoteSession}, {Environment.ProcessorCount} logical CPUs, {num2 / 1048576} MB RAM, battery = {OnBattery} -> animations {(flag ? "off (low-end PC)" : "allowed")}.");
		return flag;
	}

	private static bool ReadOnBattery()
	{
		if (GetSystemPowerStatus(out var status))
		{
			return status.ACLineStatus == 0;
		}
		return false;
	}

	[DllImport("kernel32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetSystemPowerStatus(out SystemPowerStatus status);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);
}
