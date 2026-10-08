using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.Services;

public static class ProfileCommandLine
{
	public const int Ok = 0;

	public const int PartialFailure = 1;

	public const int InvalidProfile = 2;

	public const int NotElevated = 4;

	public static async Task<int> RunAsync(string path, bool silent)
	{
		ILocalizer loc = AppServices.Localizer;
		string fullPath = Path.GetFullPath(path);
		AppServices.Logger.Info($"Command line: applying profile '{fullPath}' (silent = {silent}).");
		if (!Elevation.IsElevated)
		{
			return RelaunchElevated(fullPath, silent);
		}
		OptimizationProfile profile = AppServices.Profiles.Load(fullPath, out string error);
		if (error.Length > 0)
		{
			Tell(silent, error, MessageBoxImage.Hand);
			return 2;
		}
		ProfileApplyReport profileApplyReport = await AppServices.Profiles.ApplyAsync(profile).ConfigureAwait(continueOnCapturedContext: true);
		string text = loc.Format("profile.applied", profileApplyReport.TweaksApplied, profileApplyReport.ServicesApplied);
		if (!profileApplyReport.Success)
		{
			text = text + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, profileApplyReport.Errors.Take(10));
		}
		Tell(silent, text, profileApplyReport.Success ? MessageBoxImage.Asterisk : MessageBoxImage.Exclamation);
		return (!profileApplyReport.Success) ? 1 : 0;
	}

	private static int RelaunchElevated(string path, bool silent)
	{
		if (!PackageContext.IsPackaged)
		{
			string processPath = Environment.ProcessPath;
			if (processPath != null)
			{
				try
				{
					using Process process = Process.Start(new ProcessStartInfo(processPath)
					{
						UseShellExecute = true,
						Verb = "runas",
						Arguments = "--apply-profile \"" + path + "\"" + (silent ? " --silent" : string.Empty)
					});
					process?.WaitForExit();
					return process?.ExitCode ?? 4;
				}
				catch (Win32Exception)
				{
					return 4;
				}
			}
		}
		Tell(silent, AppServices.Localizer["profile.cli.notElevated"], MessageBoxImage.Exclamation);
		return 4;
	}

	private static void Tell(bool silent, string message, MessageBoxImage image)
	{
		AppServices.Logger.Info("Command line: " + message);
		if (!silent)
		{
			MessageBox.Show(message, "GhostEye", MessageBoxButton.OK, image);
		}
	}
}
