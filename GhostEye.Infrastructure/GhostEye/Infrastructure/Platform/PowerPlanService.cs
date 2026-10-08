using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GhostEye.Core.Localization;

namespace GhostEye.Infrastructure.Platform;

public sealed class PowerPlanService(ILocalizer loc) : IPowerPlanService
{
	private static readonly Guid HighPerformance = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");

	private const uint ErrorSuccess = 0u;

	private const uint ErrorMoreData = 234u;

	private const uint ErrorNoMoreItems = 259u;

	private const uint AccessScheme = 16u;

	public Guid HighPerformanceGuid => HighPerformance;

	[DllImport("powrprof.dll", SetLastError = true)]
	private static extern uint PowerGetActiveScheme(nint userRootPowerKey, out nint activePolicyGuid);

	[DllImport("powrprof.dll", SetLastError = true)]
	private static extern uint PowerSetActiveScheme(nint userRootPowerKey, ref Guid schemeGuid);

	[DllImport("powrprof.dll", SetLastError = true)]
	private static extern uint PowerEnumerate(nint rootPowerKey, nint schemeGuid, nint subGroupOfPowerSettingsGuid, uint accessFlags, uint index, [Out] byte[]? buffer, ref uint bufferSize);

	[DllImport("powrprof.dll", SetLastError = true)]
	private static extern uint PowerReadFriendlyName(nint rootPowerKey, ref Guid schemeGuid, nint subGroupOfPowerSettingsGuid, nint powerSettingGuid, [Out] byte[]? buffer, ref uint bufferSize);

	[DllImport("kernel32.dll")]
	private static extern nint LocalFree(nint hMem);

	public Guid? GetActiveScheme()
	{
		if (PowerGetActiveScheme(IntPtr.Zero, out var activePolicyGuid) != 0 || activePolicyGuid == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			return Marshal.PtrToStructure<Guid>(activePolicyGuid);
		}
		finally
		{
			LocalFree(activePolicyGuid);
		}
	}

	public bool TrySetActiveScheme(Guid schemeGuid, out string error)
	{
		uint num = PowerSetActiveScheme(IntPtr.Zero, ref schemeGuid);
		if (num == 0)
		{
			error = string.Empty;
			return true;
		}
		error = ((num == 5) ? loc["result.powerPlan.accessDenied"] : loc.Format("result.powerPlan.setFailed", num));
		return false;
	}

	public IReadOnlyList<PowerPlan> GetSchemes()
	{
		List<PowerPlan> list = new List<PowerPlan>();
		uint num = 0u;
		while (true)
		{
			uint bufferSize = 0u;
			uint num2 = PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 16u, num, null, ref bufferSize);
			if (num2 == 259)
			{
				break;
			}
			bool flag = ((num2 == 0 || num2 == 234) ? true : false);
			if (!flag || bufferSize == 0)
			{
				break;
			}
			byte[] array = new byte[bufferSize];
			if (PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 16u, num, array, ref bufferSize) != 0)
			{
				break;
			}
			Guid guid = new Guid(array);
			list.Add(new PowerPlan(guid, GetSchemeName(guid) ?? guid.ToString()));
			num++;
		}
		return list;
	}

	public string? GetSchemeName(Guid schemeGuid)
	{
		uint bufferSize = 0u;
		uint num = PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, null, ref bufferSize);
		bool flag = ((num == 0 || num == 234) ? true : false);
		if (!flag || bufferSize == 0)
		{
			return null;
		}
		byte[] array = new byte[bufferSize];
		if (PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, array, ref bufferSize) != 0)
		{
			return null;
		}
		return Encoding.Unicode.GetString(array).TrimEnd('\0');
	}

	public bool IsHighPerformanceAvailable()
	{
		return GetSchemes().Any((PowerPlan p) => p.Id == HighPerformance);
	}
}
