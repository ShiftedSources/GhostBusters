using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace GhostEye.App.Services;

public static class SingleInstance
{
	private const string MutexName = "Local\\GhostEye.SingleInstance";

	private const string EventName = "Local\\GhostEye.Activate";

	private static Mutex? _mutex;

	private static EventWaitHandle? _activate;

	private static Action? _onActivate;

	public static bool TryAcquire()
	{
		try
		{
			_mutex = new Mutex(initiallyOwned: true, "Local\\GhostEye.SingleInstance", out var createdNew);
			if (createdNew)
			{
				return true;
			}
			_mutex.Dispose();
			_mutex = null;
		}
		catch (UnauthorizedAccessException)
		{
		}
		try
		{
			using EventWaitHandle eventWaitHandle = EventWaitHandle.OpenExisting("Local\\GhostEye.Activate");
			eventWaitHandle.Set();
		}
		catch (Exception ex2) when ((ex2 is WaitHandleCannotBeOpenedException || ex2 is UnauthorizedAccessException) ? true : false)
		{
		}
		return false;
	}

	public static void Listen(Action onActivate)
	{
		_onActivate = onActivate;
		EventWaitHandleSecurity eventWaitHandleSecurity = new EventWaitHandleSecurity();
		eventWaitHandleSecurity.AddAccessRule(new EventWaitHandleAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), EventWaitHandleRights.Modify | EventWaitHandleRights.Synchronize, AccessControlType.Allow));
		_activate = EventWaitHandleAcl.Create(initialState: false, EventResetMode.AutoReset, "Local\\GhostEye.Activate", out var _, eventWaitHandleSecurity);
		Thread thread = new Thread(() =>
		{
			while (true)
			{
				EventWaitHandle activate = _activate;
				if (activate == null)
				{
					break;
				}
				try
				{
					activate.WaitOne();
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				Application.Current?.Dispatcher.BeginInvoke(onActivate);
			}
		});
		thread.IsBackground = true;
		thread.Name = "GhostEye activation listener";
		thread.Start();
	}

	public static void Release()
	{
		try
		{
			_mutex?.ReleaseMutex();
		}
		catch (ApplicationException)
		{
		}
		_mutex?.Dispose();
		_mutex = null;
		EventWaitHandle? activate = _activate;
		_activate = null;
		activate?.Dispose();
	}

	public static void Restore()
	{
		if (_mutex == null && TryAcquire())
		{
			Action onActivate = _onActivate;
			if (onActivate != null)
			{
				Listen(onActivate);
			}
		}
	}
}
