using System.Runtime.InteropServices;

namespace GhostEye.Infrastructure.Platform;

internal static class NativeServices
{
	public const uint ScManagerConnect = 1u;

	public const uint ServiceQueryConfig = 1u;

	public const uint ServiceChangeConfig = 2u;

	public const uint ServiceNoChange = uint.MaxValue;

	public const int ErrorAccessDenied = 5;

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern nint OpenSCManagerW(string? machineName, string? databaseName, uint desiredAccess);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern nint OpenServiceW(nint scManager, string serviceName, uint desiredAccess);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ChangeServiceConfigW(nint service, uint serviceType, uint startType, uint errorControl, string? binaryPathName, string? loadOrderGroup, nint tagId, string? dependencies, string? serviceStartName, string? password, string? displayName);

	[DllImport("advapi32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CloseServiceHandle(nint handle);
}
