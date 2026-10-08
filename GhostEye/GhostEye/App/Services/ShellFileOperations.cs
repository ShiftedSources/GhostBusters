using System.IO;
using System.Runtime.InteropServices;

namespace GhostEye.App.Services;

internal static class ShellFileOperations
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ShFileOpStruct
	{
		public nint hwnd;

		public uint wFunc;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string pFrom;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string? pTo;

		public ushort fFlags;

		[MarshalAs(UnmanagedType.Bool)]
		public bool fAnyOperationsAborted;

		public nint hNameMappings;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string? lpszProgressTitle;
	}

	private const uint FoDelete = 3u;

	private const ushort FofSilent = 4;

	private const ushort FofNoConfirmation = 16;

	private const ushort FofAllowUndo = 64;

	private const ushort FofNoErrorUi = 1024;

	public static void SendToRecycleBin(string path)
	{
		ShFileOpStruct fileOp = new ShFileOpStruct
		{
			wFunc = 3u,
			pFrom = path + "\0\0",
			fFlags = 1108
		};
		int num = SHFileOperation(ref fileOp);
		if (num != 0 || fileOp.fAnyOperationsAborted)
		{
			throw new IOException($"The file could not be moved to the Recycle Bin (code {num}).");
		}
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern int SHFileOperation(ref ShFileOpStruct fileOp);
}
