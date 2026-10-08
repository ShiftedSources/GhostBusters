using System;
using System.Management;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;

namespace GhostEye.Backup;

[SupportedOSPlatform("windows")]
public sealed class RestorePointService : IRestorePointService
{
	private const int ApplicationInstall = 0;

	private const int ModifySettings = 12;

	private const int BeginSystemChange = 100;

	private readonly IAppLogger _logger;

	public RestorePointService(IAppLogger logger)
	{
		_logger = logger;
	}

	public Task<long?> CreateAsync(string description, CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			try
			{
				ManagementClass managementClass = new ManagementClass("\\\\.\\root\\default:SystemRestore");
				try
				{
					ManagementBaseObject methodParameters = managementClass.GetMethodParameters("CreateRestorePoint");
					try
					{
						methodParameters["Description"] = "GhostEye — " + description;
						methodParameters["RestorePointType"] = 12;
						methodParameters["EventType"] = 100;
						ManagementBaseObject managementBaseObject = managementClass.InvokeMethod("CreateRestorePoint", methodParameters, null);
						try
						{
							uint num = Convert.ToUInt32(managementBaseObject["ReturnValue"]);
							if (num != 0)
							{
								_logger.Warn($"CreateRestorePoint returned {num}: restore point not created.");
								return (long?)null;
							}
							return GetLatestSequenceNumber();
						}
						finally
						{
							((IDisposable)managementBaseObject)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)methodParameters)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)managementClass)?.Dispose();
				}
			}
			catch (ManagementException ex)
			{
				_logger.Warn("Restore point unavailable: " + ex.Message);
				return (long?)null;
			}
			catch (UnauthorizedAccessException)
			{
				_logger.Warn("Restore point not created: insufficient permissions.");
				return (long?)null;
			}
		}, ct);
	}

	public Task<bool> IsSystemRestoreEnabledAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			try
			{
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("\\\\.\\root\\default", "SELECT SequenceNumber FROM SystemRestore");
				try
				{
					using ManagementObjectCollection managementObjectCollection = managementObjectSearcher.Get();
					_ = managementObjectCollection.Count;
					return true;
				}
				finally
				{
					((IDisposable)managementObjectSearcher)?.Dispose();
				}
			}
			catch (ManagementException)
			{
				return false;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}, ct);
	}

	private static long? GetLatestSequenceNumber()
	{
		long? num3;
		try
		{
			ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("\\\\.\\root\\default", "SELECT SequenceNumber FROM SystemRestore");
			try
			{
				long? num = null;
				foreach (ManagementBaseObject item in managementObjectSearcher.Get())
				{
					ManagementBaseObject managementBaseObject = item;
					try
					{
						long num2 = Convert.ToInt64(item["SequenceNumber"]);
						if (!num.HasValue)
						{
							goto IL_0063;
						}
						num3 = num;
						if (num2 > num3)
						{
							goto IL_0063;
						}
						goto end_IL_0030;
						IL_0063:
						num = num2;
						end_IL_0030:;
					}
					finally
					{
						((IDisposable)managementBaseObject)?.Dispose();
					}
				}
				num3 = num;
			}
			finally
			{
				((IDisposable)managementObjectSearcher)?.Dispose();
			}
		}
		catch (ManagementException)
		{
			num3 = null;
		}
		return num3;
	}
}
