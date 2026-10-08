using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using GhostEye.App.Services;
using GhostEye.Backup;

namespace GhostEye.App;

public partial class App : Application
{
	public const string TrayArgument = "--tray";

	public const string ApplyProfileArgument = "--apply-profile";

	public static bool IsExiting { get; private set; }

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		string text = ReadArgument(e.Args, "--apply-profile");
		bool flag = false;
		if (text == null && !flag && !SingleInstance.TryAcquire())
		{
			IsExiting = true;
			Shutdown();
			return;
		}
		AppServices.Initialize();
		DispatcherUnhandledException += OnUnhandledException;
		if (text != null)
		{
			ShutdownMode = ShutdownMode.OnExplicitShutdown;
			int exitCode = await ProfileCommandLine.RunAsync(text, e.Args.Contains("--silent", StringComparer.OrdinalIgnoreCase));
			IsExiting = true;
			Shutdown(exitCode);
			return;
		}
		ShutdownMode = ShutdownMode.OnExplicitShutdown;
		SessionEnding += (object _, SessionEndingCancelEventArgs _) =>
		{
			IsExiting = true;
		};
		ThemeManager.ApplyAtStartup();
		ShellWindow shellWindow = (ShellWindow)(MainWindow = new ShellWindow());
		if (!e.Args.Contains("--tray", StringComparer.OrdinalIgnoreCase) || !AppServices.Settings.Current.RunInBackground)
		{
			shellWindow.Show();
		}
		if (!flag)
		{
			SingleInstance.Listen(ShowMainWindow);
			TrayIcon.Initialize();
			BackgroundAgent.Start();
			BackgroundAgent.RefreshStatus();
			ThemeManager.StartSeasonWatch();
		}
		string text2 = ReadArgument(e.Args, "--page") ?? PendingPage.Take();
		if (text2 != null)
		{
			AppServices.Shell.NavigateTo(text2);
		}
		AdminGate.WarnIfElevatedAsOtherUser();
	}

	public static void ShowMainWindow()
	{
		Window window = Application.Current?.MainWindow;
		if (window != null)
		{
			if (!window.IsVisible)
			{
				window.Show();
			}
			if (window.WindowState == WindowState.Minimized)
			{
				window.WindowState = WindowState.Normal;
			}
			window.Activate();
			window.Topmost = true;
			window.Topmost = false;
			window.Focus();
		}
	}

	public static void RequestExit()
	{
		IsExiting = true;
		Application.Current?.Shutdown();
	}

	protected override void OnExit(ExitEventArgs e)
	{
		BackgroundAgent.Stop();
		TrayIcon.Dispose();
		SingleInstance.Release();
		base.OnExit(e);
	}

	private static string? ReadArgument(string[] args, string name)
	{
		int num = Array.FindIndex(args, (string a) => a.Equals(name, StringComparison.OrdinalIgnoreCase));
		if (num < 0 || num + 1 >= args.Length)
		{
			return null;
		}
		return args[num + 1];
	}

	private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		AppServices.Logger.Error("Unhandled exception", e.Exception);
		MessageBox.Show(AppServices.Localizer.Format("app.unexpectedError", e.Exception.Message, AppPaths.Logs), "GhostEye", MessageBoxButton.OK, MessageBoxImage.Hand);
		e.Handled = true;
	}
}
