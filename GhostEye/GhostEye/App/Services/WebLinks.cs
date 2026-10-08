using System;
using System.ComponentModel;
using System.Diagnostics;

namespace GhostEye.App.Services;

public static class WebLinks
{
	public static void OpenWebsite()
	{
		Open("https://ghosteye.store");
	}

	private static void Open(string url)
	{
		try
		{
			Process.Start(new ProcessStartInfo(url)
			{
				UseShellExecute = true
			});
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
		{
			AppServices.Logger.Error("Could not open " + url, ex);
		}
	}
}
