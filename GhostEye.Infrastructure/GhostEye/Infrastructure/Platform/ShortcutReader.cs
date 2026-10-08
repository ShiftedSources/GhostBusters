using System;
using System.Runtime.InteropServices;
using Microsoft.CSharp.RuntimeBinder;

namespace GhostEye.Infrastructure.Platform;

internal static class ShortcutReader
{
	public static string? TryGetTarget(string lnkPath)
	{
		object obj = null;
		try
		{
			Type typeFromProgID = Type.GetTypeFromProgID("WScript.Shell");
			if ((object)typeFromProgID == null)
			{
				return null;
			}
			obj = Activator.CreateInstance(typeFromProgID);
			dynamic val = obj;
			dynamic val2 = val.CreateShortcut(lnkPath);
			string text = val2.TargetPath;
			string text2 = val2.Arguments;
			return string.IsNullOrWhiteSpace(text) ? null : ("\"" + text + "\" " + text2).Trim();
		}
		catch (Exception ex) when ((ex is COMException || ex is RuntimeBinderException || ex is InvalidOperationException || ex is ArgumentException) ? true : false)
		{
			return null;
		}
		finally
		{
			if (obj != null && Marshal.IsComObject(obj))
			{
				Marshal.ReleaseComObject(obj);
			}
		}
	}
}
