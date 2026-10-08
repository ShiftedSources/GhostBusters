using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using GhostEye.Core.Localization;

namespace GhostEye.Infrastructure.Platform;

public sealed class ServiceManagerService : IServiceManagerService
{
	private const string ServicesKeyRoot = "HKLM\\SYSTEM\\CurrentControlSet\\Services";

	private static readonly TimeSpan StateChangeTimeout = TimeSpan.FromSeconds(20.0);

	private readonly IRegistryService _registry;

	private readonly ILocalizer _loc;

	public ServiceManagerService(IRegistryService registry, ILocalizer loc)
	{
		_registry = registry;
		_loc = loc;
	}

	public IReadOnlyList<ServiceInfo> GetServices()
	{
		List<ServiceInfo> list = new List<ServiceInfo>();
		ServiceController[] services = ServiceController.GetServices();
		foreach (ServiceController serviceController in services)
		{
			using (serviceController)
			{
				try
				{
					list.Add(new ServiceInfo(serviceController.ServiceName, serviceController.DisplayName, serviceController.Status, GetStartupMode(serviceController.ServiceName) ?? ServiceStartupMode.Manual));
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		return list;
	}

	public ServiceInfo? GetService(string serviceName)
	{
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			return new ServiceInfo(serviceController.ServiceName, serviceController.DisplayName, serviceController.Status, GetStartupMode(serviceName) ?? ServiceStartupMode.Manual);
		}
		catch (InvalidOperationException)
		{
			return null;
		}
	}

	public ServiceStartupMode? GetStartupMode(string serviceName)
	{
		int? num = _registry.ReadDword("HKLM\\SYSTEM\\CurrentControlSet\\Services\\" + serviceName, "Start");
		if (num.HasValue)
		{
			return (ServiceStartupMode)num.Value;
		}
		return null;
	}

	public bool TrySetStartupMode(string serviceName, ServiceStartupMode mode, out string error)
	{
		nint num = NativeServices.OpenSCManagerW(null, null, 1u);
		if (num == IntPtr.Zero)
		{
			error = DescribeError(Marshal.GetLastWin32Error());
			return false;
		}
		try
		{
			nint num2 = NativeServices.OpenServiceW(num, serviceName, 3u);
			if (num2 == IntPtr.Zero)
			{
				error = DescribeError(Marshal.GetLastWin32Error());
				return false;
			}
			try
			{
				if (!NativeServices.ChangeServiceConfigW(num2, uint.MaxValue, (uint)mode, uint.MaxValue, null, null, IntPtr.Zero, null, null, null, null))
				{
					error = DescribeError(Marshal.GetLastWin32Error());
					return false;
				}
				error = string.Empty;
				return true;
			}
			finally
			{
				NativeServices.CloseServiceHandle(num2);
			}
		}
		finally
		{
			NativeServices.CloseServiceHandle(num);
		}
	}

	private string DescribeError(int code)
	{
		if (code == 5)
		{
			return Elevation.IsElevated ? _loc["services.error.protected"] : _loc["result.registry.accessDenied"];
		}
		return new Win32Exception(code).Message;
	}

	public bool TryStop(string serviceName, out string error)
	{
		bool flag;
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			ServiceControllerStatus status = serviceController.Status;
			flag = ((status == ServiceControllerStatus.Stopped || status == ServiceControllerStatus.StopPending) ? true : false);
			if (flag)
			{
				error = string.Empty;
				flag = true;
			}
			else if (!serviceController.CanStop)
			{
				error = "Service '" + serviceName + "' cannot be stopped.";
				flag = false;
			}
			else
			{
				serviceController.Stop();
				serviceController.WaitForStatus(ServiceControllerStatus.Stopped, StateChangeTimeout);
				error = string.Empty;
				flag = true;
			}
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception || ex is System.ServiceProcess.TimeoutException) ? true : false)
		{
			error = "Could not stop service '" + serviceName + "': " + ex.Message;
			flag = false;
		}
		return flag;
	}

	public bool TryStart(string serviceName, out string error)
	{
		bool flag;
		try
		{
			using ServiceController serviceController = new ServiceController(serviceName);
			ServiceControllerStatus status = serviceController.Status;
			flag = ((status == ServiceControllerStatus.StartPending || status == ServiceControllerStatus.Running) ? true : false);
			if (flag)
			{
				error = string.Empty;
				flag = true;
			}
			else
			{
				serviceController.Start();
				serviceController.WaitForStatus(ServiceControllerStatus.Running, StateChangeTimeout);
				error = string.Empty;
				flag = true;
			}
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception || ex is System.ServiceProcess.TimeoutException) ? true : false)
		{
			error = "Could not start service '" + serviceName + "': " + ex.Message;
			flag = false;
		}
		return flag;
	}
}
