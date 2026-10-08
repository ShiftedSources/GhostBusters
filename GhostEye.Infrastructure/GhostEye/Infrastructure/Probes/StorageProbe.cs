using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

[SupportedOSPlatform("windows")]
public sealed class StorageProbe(IProcessRunner processRunner, ILocalizer loc) : IAnalysisProbe
{
	public string Id => "perf.storage";

	public AnalysisGroup Group => AnalysisGroup.Performance;

	public async Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		DriveInfo driveInfo = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\");
		double freeGb = (double)driveInfo.AvailableFreeSpace / 1073741824.0;
		double num = (double)driveInfo.TotalSize / 1073741824.0;
		double freePercent = freeGb / num * 100.0;
		string mediaType = await ReadMediaTypeAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		bool? flag = await IsTrimEnabledAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		AnalysisStatus analysisStatus = ((freePercent < 5.0) ? AnalysisStatus.Critical : ((freePercent < 10.0 || flag == false) ? AnalysisStatus.Warning : AnalysisStatus.Ok));
		AnalysisStatus analysisStatus2 = analysisStatus;
		List<string> list = new List<string> { mediaType };
		if (flag.HasValue)
		{
			list.Add(flag.Value ? loc["probe.storage.trimOn"] : loc["probe.storage.trimOff"]);
		}
		string text = string.Join(", ", list);
		string description = analysisStatus2 switch
		{
			AnalysisStatus.Critical => loc.Format("probe.storage.critical", text), 
			AnalysisStatus.Warning => (flag != false) ? loc.Format("probe.storage.lowSpace", text) : loc.Format("probe.storage.noTrim", text), 
			_ => loc.Format("probe.storage.ok", text), 
		};
		return new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.storage.name"],
			Measurement = loc.Format("probe.storage.free", freeGb.ToString("0", loc.Culture) + " GB"),
			Status = analysisStatus2,
			Description = description,
			SuggestedTweakId = ((flag == false) ? "perf.storage-optimize" : null)
		};
	}

	private async Task<string> ReadMediaTypeAsync(CancellationToken ct)
	{
		return await Task.Run(() =>
		{
			try
			{
				using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("\\\\.\\root\\Microsoft\\Windows\\Storage", "SELECT MediaType FROM MSFT_PhysicalDisk");
				using ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator = managementObjectSearcher.Get().GetEnumerator();
				if (managementObjectEnumerator.MoveNext())
				{
					ManagementBaseObject current = managementObjectEnumerator.Current;
					using (current)
					{
						return Convert.ToInt32(current["MediaType"]) switch
						{
							3 => loc["probe.storage.hdd"], 
							4 => loc["probe.storage.ssd"], 
							5 => loc["probe.storage.scm"], 
							_ => loc["probe.storage.unknown"], 
						};
					}
				}
			}
			catch (ManagementException)
			{
			}
			return loc["probe.storage.unknown"];
		}, ct).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<bool?> IsTrimEnabledAsync(CancellationToken ct)
	{
		ProcessResult processResult = await processRunner.RunAsync("fsutil.exe", "behavior query DisableDeleteNotify", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!processResult.Success)
		{
			return null;
		}
		List<int> list = (from line in processResult.StandardOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			select line.Split('=').LastOrDefault()?.Trim() into value
			where int.TryParse(value, out var _)
			select value).Select(int.Parse).ToList();
		return (list.Count == 0) ? ((bool?)null) : new bool?(list.All((int v) => v == 0));
	}
}
