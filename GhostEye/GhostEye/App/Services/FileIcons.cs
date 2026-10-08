using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GhostEye.App.Services;

public static class FileIcons
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ShFileInfo
	{
		public nint hIcon;

		public int iIcon;

		public uint dwAttributes;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string szDisplayName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
		public string szTypeName;
	}

	private const uint ShgfiIcon = 256u;

	private const uint ShgfiLargeIcon = 0u;

	private static readonly ConcurrentDictionary<string, ImageSource?> Cache = new ConcurrentDictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

	public static ImageSource? For(string? path)
	{
		if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
		{
			return null;
		}
		return Cache.GetOrAdd(path, Load);
	}

	private static ImageSource? Load(string path)
	{
		ShFileInfo psfi = default;
		if (SHGetFileInfo(path, 0u, ref psfi, (uint)Marshal.SizeOf(psfi), 256u) == IntPtr.Zero || psfi.hIcon == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(psfi.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			bitmapSource.Freeze();
			return bitmapSource;
		}
		catch (Exception ex) when ((ex is COMException || ex is ArgumentException || ex is InvalidOperationException) ? true : false)
		{
			return null;
		}
		finally
		{
			DestroyIcon(psfi.hIcon);
		}
	}

	[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
	private static extern nint SHGetFileInfo(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi, uint cbFileInfo, uint uFlags);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DestroyIcon(nint hIcon);
}
