using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace GhostEye.Infrastructure.Health;

[SupportedOSPlatform("windows")]
public sealed class SystemMonitor : IDisposable
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private readonly struct MemoryReader
	{
		public (long Total, long Available) Read()
		{
			MemoryStatusEx buffer = new MemoryStatusEx
			{
				dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>()
			};
			if (!GlobalMemoryStatusEx(ref buffer))
			{
				return (Total: 0L, Available: 0L);
			}
			return (Total: (long)buffer.ullTotalPhys, Available: (long)buffer.ullAvailPhys);
		}
	}

	private struct MemoryStatusEx
	{
		public uint dwLength;

		public uint dwMemoryLoad;

		public ulong ullTotalPhys;

		public ulong ullAvailPhys;

		public ulong ullTotalPageFile;

		public ulong ullAvailPageFile;

		public ulong ullTotalVirtual;

		public ulong ullAvailVirtual;

		public ulong ullAvailExtendedVirtual;
	}

	private readonly PerformanceCounter? _cpu;

	private readonly PerformanceCounter? _disk;

	private readonly List<PerformanceCounter> _network = new List<PerformanceCounter>();

	private List<PerformanceCounter> _gpu = new List<PerformanceCounter>();

	private DateTime _gpuRefreshed = DateTime.MinValue;

	private DateTime _temperaturesRead = DateTime.MinValue;

	private (double? Gpu, double? Util, string? Name) _nvidia;

	private double? _cpuTemperature;

	private readonly string? _nvidiaSmi;

	public SystemMonitor()
	{
		_cpu = TryCounter("Processor Information", "% Processor Utility", "_Total") ?? TryCounter("Processor", "% Processor Time", "_Total");
		_disk = TryCounter("PhysicalDisk", "% Idle Time", "_Total");
		try
		{
			string[] instanceNames = new PerformanceCounterCategory("Network Interface").GetInstanceNames();
			foreach (string instance in instanceNames)
			{
				PerformanceCounter performanceCounter = TryCounter("Network Interface", "Bytes Total/sec", instance);
				if (performanceCounter != null)
				{
					_network.Add(performanceCounter);
				}
			}
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is UnauthorizedAccessException || ex is Win32Exception) ? true : false)
		{
		}
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "nvidia-smi.exe");
		_nvidiaSmi = (File.Exists(text) ? text : null);
		Sample();
	}

	public MonitorSample Sample()
	{
		(long, long) tuple = default(MemoryReader).Read();
		RefreshGpuCounters();
		RefreshTemperatures();
		double? gpuPercent = null;
		if (_gpu.Count > 0)
		{
			gpuPercent = Math.Min(100.0, _gpu.Sum((PerformanceCounter c) => SafeNext(c)));
		}
		else
		{
			double? item = _nvidia.Util;
			if (item.HasValue)
			{
				double valueOrDefault = item.GetValueOrDefault();
				gpuPercent = valueOrDefault;
			}
		}
		return new MonitorSample
		{
			CpuPercent = Math.Clamp(SafeNext(_cpu), 0.0, 100.0),
			MemoryTotalBytes = tuple.Item1,
			MemoryUsedBytes = tuple.Item1 - tuple.Item2,
			MemoryPercent = ((tuple.Item1 == 0L) ? 0.0 : (100.0 * (double)(tuple.Item1 - tuple.Item2) / (double)tuple.Item1)),
			GpuPercent = gpuPercent,
			DiskPercent = ((_disk == null) ? 0.0 : Math.Clamp(100.0 - SafeNext(_disk), 0.0, 100.0)),
			NetworkBitsPerSecond = _network.Sum((PerformanceCounter c) => SafeNext(c)) * 8.0,
			CpuTemperatureC = _cpuTemperature,
			GpuTemperatureC = _nvidia.Gpu,
			GpuName = _nvidia.Name
		};
	}

	private void RefreshGpuCounters()
	{
		if (DateTime.UtcNow - _gpuRefreshed < TimeSpan.FromSeconds(10.0))
		{
			return;
		}
		_gpuRefreshed = DateTime.UtcNow;
		foreach (PerformanceCounter item in _gpu)
		{
			item.Dispose();
		}
		_gpu = new List<PerformanceCounter>();
		try
		{
			foreach (string item2 in from i in new PerformanceCounterCategory("GPU Engine").GetInstanceNames()
				where i.EndsWith("engtype_3D", StringComparison.OrdinalIgnoreCase)
				select i)
			{
				PerformanceCounter performanceCounter = TryCounter("GPU Engine", "Utilization Percentage", item2);
				if (performanceCounter != null)
				{
					performanceCounter.NextValue();
					_gpu.Add(performanceCounter);
				}
			}
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is UnauthorizedAccessException || ex is Win32Exception) ? true : false)
		{
		}
	}

	private void RefreshTemperatures()
	{
		if (!(DateTime.UtcNow - _temperaturesRead < TimeSpan.FromSeconds(5.0)))
		{
			_temperaturesRead = DateTime.UtcNow;
			_nvidia = ReadNvidia();
			_cpuTemperature = ReadAcpiTemperature();
		}
	}

	private (double?, double?, string?) ReadNvidia()
	{
		if (_nvidiaSmi == null)
		{
			return (null, null, null);
		}
		try
		{
			using Process process = Process.Start(new ProcessStartInfo(_nvidiaSmi, "--query-gpu=temperature.gpu,utilization.gpu,name --format=csv,noheader,nounits")
			{
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			});
			if (process == null)
			{
				return (null, null, null);
			}
			string? text = process.StandardOutput.ReadLine();
			process.WaitForExit(3000);
			string[] array = text?.Split(',', StringSplitOptions.TrimEntries);
			if (array == null || array.Length < 3)
			{
				return (null, null, null);
			}
			double result;
			double result2;
			return (double.TryParse(array[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? new double?(result) : ((double?)null), double.TryParse(array[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result2) ? new double?(result2) : ((double?)null), array[2]);
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException || ex is IOException) ? true : false)
		{
			return (null, null, null);
		}
	}

	private static double? ReadAcpiTemperature()
	{
		double? num3;
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("root\\WMI", "SELECT CurrentTemperature FROM MSAcpi_ThermalZoneTemperature");
			double? num = null;
			foreach (ManagementBaseObject item in managementObjectSearcher.Get())
			{
				using (item)
				{
					double num2 = Convert.ToDouble(item["CurrentTemperature"], CultureInfo.InvariantCulture) / 10.0 - 273.15;
					if (num2 > 5.0 && num2 < 125.0)
					{
						if (!num.HasValue)
						{
							goto IL_0099;
						}
						num3 = num;
						if (num2 > num3)
						{
							goto IL_0099;
						}
					}
					goto end_IL_0033;
					IL_0099:
					num = Math.Round(num2, 1);
					end_IL_0033:;
				}
			}
			num3 = num;
		}
		catch (Exception ex) when ((ex is ManagementException || ex is UnauthorizedAccessException || ex is COMException) ? true : false)
		{
			num3 = null;
		}
		return num3;
	}

	private static PerformanceCounter? TryCounter(string category, string counter, string instance)
	{
		try
		{
			PerformanceCounter performanceCounter = new PerformanceCounter(category, counter, instance, readOnly: true);
			performanceCounter.NextValue();
			return performanceCounter;
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is UnauthorizedAccessException || ex is Win32Exception) ? true : false)
		{
			return null;
		}
	}

	private static double SafeNext(PerformanceCounter? counter)
	{
		if (counter == null)
		{
			return 0.0;
		}
		try
		{
			return counter.NextValue();
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception) ? true : false)
		{
			return 0.0;
		}
	}

	public void Dispose()
	{
		_cpu?.Dispose();
		_disk?.Dispose();
		foreach (PerformanceCounter item in _network.Concat(_gpu))
		{
			item.Dispose();
		}
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);
}
