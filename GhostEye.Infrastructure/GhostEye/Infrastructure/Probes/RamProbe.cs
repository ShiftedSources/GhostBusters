using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;

namespace GhostEye.Infrastructure.Probes;

[SupportedOSPlatform("windows")]
public sealed class RamProbe(ILocalizer loc) : IAnalysisProbe
{
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

	public string Id => "perf.ram";

	public AnalysisGroup Group => AnalysisGroup.Performance;

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

	public static (ulong Used, ulong Total)? Read()
	{
		MemoryStatusEx buffer = new MemoryStatusEx
		{
			dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>()
		};
		if (!GlobalMemoryStatusEx(ref buffer))
		{
			return null;
		}
		return (buffer.ullTotalPhys - buffer.ullAvailPhys, buffer.ullTotalPhys);
	}

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			(ulong, ulong)? tuple = Read();
			if (!tuple.HasValue)
			{
				return new AnalysisItem
				{
					Id = Id,
					Group = Group,
					Name = loc["probe.ram.name"],
					Description = loc["probe.ram.unreadable"],
					Status = AnalysisStatus.Warning
				};
			}
			(ulong, ulong) value = tuple.Value;
			ulong item = value.Item1;
			ulong item2 = value.Item2;
			double num = (double)item * 100.0 / (double)item2;
			List<string> values = (from p in (from p in Process.GetProcesses().Select((Process p) =>
					{
						try
						{
							return (Name: p.ProcessName, Bytes: p.WorkingSet64);
						}
						catch (InvalidOperationException)
						{
							return (Name: string.Empty, Bytes: 0L);
						}
						finally
						{
							p.Dispose();
						}
					})
					where p.Name.Length > 0
					orderby p.Bytes descending
					select p).Take(3)
				select p.Name + " (" + FormatGb(p.Bytes) + ")").ToList();
			AnalysisStatus analysisStatus = ((num >= 85.0) ? AnalysisStatus.Critical : ((num >= 70.0) ? AnalysisStatus.Warning : AnalysisStatus.Ok));
			AnalysisStatus analysisStatus2 = analysisStatus;
			string text = string.Join(", ", values);
			return new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.ram.name"],
				Measurement = FormatGb((long)item) + " / " + FormatGb((long)item2),
				Status = analysisStatus2,
				Description = ((analysisStatus2 == AnalysisStatus.Ok) ? loc.Format("probe.ram.ok", text) : loc.Format("probe.ram.warn", text)),
				SuggestedTweakId = ((analysisStatus2 == AnalysisStatus.Ok) ? null : "perf.startup-cleanup")
			};
		}, ct);
	}

	private string FormatGb(long bytes)
	{
		return ((double)bytes / 1073741824.0).ToString("0.0", loc.Culture) + " GB";
	}
}
