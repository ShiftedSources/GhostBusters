using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GhostEye.App.ViewModels;
using GhostEye.Core.Localization;

namespace GhostEye.App.Services;

public static class AdminGate
{
	public const string PageArgument = "--page";

	public static bool IsElevated => ShellViewModel.IsElevated;

	public static async Task<bool> RequireAdminAsync(string? returnToPage = null)
	{
		if (IsElevated)
		{
			return true;
		}
		ILocalizer localizer = AppServices.Localizer;
		if (!(await AppServices.Dialogs.ConfirmAsync(localizer["admin.required.title"], localizer["admin.required.body"], localizer["admin.restart"])))
		{
			return false;
		}
		RestartElevated(returnToPage ?? AppServices.Shell?.Items.FirstOrDefault((NavItemViewModel i) => i.IsSelected)?.Id);
		return false;
	}

	public static void WarnIfElevatedAsOtherUser()
	{
		if (IsElevated)
		{
			string interactiveUserName = SessionUser.GetInteractiveUserName();
			string userName = Environment.UserName;
			if (interactiveUserName != null && !interactiveUserName.Equals(userName, StringComparison.OrdinalIgnoreCase))
			{
				ILocalizer localizer = AppServices.Localizer;
				AppServices.Dialogs.AlertAsync(localizer["admin.otherUser.title"], localizer.Format("admin.otherUser.body", userName, interactiveUserName));
			}
		}
	}

	public static void RestartElevated(string? page)
	{
		string processPath = Environment.ProcessPath;
		if (string.IsNullOrEmpty(processPath))
		{
			return;
		}
		bool flag = PackageContext.IsPackaged && PackageContext.ActivationPath != null;
		string fileName = (flag ? PackageContext.ActivationPath : processPath);
		if (flag)
		{
			PendingPage.Write(page);
		}
		SingleInstance.Release();
		try
		{
			Process.Start(new ProcessStartInfo(fileName)
			{
				UseShellExecute = true,
				Verb = "runas",
				Arguments = ((flag || string.IsNullOrEmpty(page)) ? string.Empty : ("--page " + page)),
				WorkingDirectory = (flag ? string.Empty : (Path.GetDirectoryName(processPath) ?? string.Empty))
			});
			App.RequestExit();
		}
		catch (Win32Exception ex)
		{
			AppServices.Logger.Info($"Elevation declined or failed: {ex.NativeErrorCode}.");
			PendingPage.Clear();
			SingleInstance.Restore();
		}
	}
}
