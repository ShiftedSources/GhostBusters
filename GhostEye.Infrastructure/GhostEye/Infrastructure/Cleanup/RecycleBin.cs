using System;
using System.Runtime.InteropServices;

namespace GhostEye.Infrastructure.Cleanup;

internal static class RecycleBin
{
	public readonly record struct Info(long Bytes, long Items);

	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	private struct ShQueryRbInfo
	{
		public int cbSize;

		public long i64Size;

		public long i64NumItems;
	}

	private const uint NoConfirmation = 1u;

	private const uint NoProgressUi = 2u;

	private const uint NoSound = 4u;

	public static Info? Query()
	{
		ShQueryRbInfo pSHQueryRBInfo = new ShQueryRbInfo
		{
			cbSize = Marshal.SizeOf<ShQueryRbInfo>()
		};
		if (SHQueryRecycleBin(null, ref pSHQueryRBInfo) != 0)
		{
			return null;
		}
		return new Info(pSHQueryRBInfo.i64Size, pSHQueryRBInfo.i64NumItems);
	}

	public static bool Empty()
	{
		int num = SHEmptyRecycleBin(IntPtr.Zero, null, 7u);
		if (num == -2147418113 || num == 0)
		{
			return true;
		}
		return false;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHQueryRecycleBin(string? pszRootPath, ref ShQueryRbInfo pSHQueryRBInfo);

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHEmptyRecycleBin(nint hwnd, string? pszRootPath, uint dwFlags);
}
