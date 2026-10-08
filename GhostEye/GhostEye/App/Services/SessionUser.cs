using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GhostEye.App.Services;

internal static class SessionUser
{
	private const int WtsUserName = 5;

	private static readonly nint CurrentServer = IntPtr.Zero;

	public static string? GetInteractiveUserName()
	{
		uint sessionId = (uint)Process.GetCurrentProcess().SessionId;
		if (!WTSQuerySessionInformation(CurrentServer, sessionId, 5, out var ppBuffer, out var _))
		{
			return null;
		}
		try
		{
			string text = Marshal.PtrToStringUni(ppBuffer);
			return string.IsNullOrWhiteSpace(text) ? null : text;
		}
		finally
		{
			WTSFreeMemory(ppBuffer);
		}
	}

	[DllImport("wtsapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool WTSQuerySessionInformation(nint hServer, uint sessionId, int wtsInfoClass, out nint ppBuffer, out int pBytesReturned);

	[DllImport("wtsapi32.dll")]
	private static extern void WTSFreeMemory(nint pMemory);
}
