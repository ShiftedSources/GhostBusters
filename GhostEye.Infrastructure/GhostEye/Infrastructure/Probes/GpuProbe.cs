using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

[SupportedOSPlatform("windows")]
public sealed class GpuProbe(IRegistryService registry, ILocalizer loc) : IAnalysisProbe
{
	public string Id => "perf.gpu";

	public AnalysisGroup Group => AnalysisGroup.Performance;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			GpuAdapter primary = GpuInfo.GetPrimary(registry);
			if ((object)primary == null)
			{
				return new AnalysisItem
				{
					Id = Id,
					Group = Group,
					Name = loc["probe.gpu.name"],
					Description = loc["probe.gpu.none"],
					Status = AnalysisStatus.Warning
				};
			}
			bool flag = primary.DriverDate.HasValue && (DateTime.Now - primary.DriverDate.Value).TotalDays > 730.0;
			string text = primary.DriverDate?.ToString("MM/yyyy", loc.Culture) ?? "?";
			return new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.gpu.name"],
				Measurement = (primary.DriverVersion ?? "?"),
				Status = (flag ? AnalysisStatus.Warning : AnalysisStatus.Ok),
				Description = (flag ? loc.Format("probe.gpu.old", primary.Name, text) : loc.Format("probe.gpu.ok", primary.Name))
			};
		}, ct);
	}
}
