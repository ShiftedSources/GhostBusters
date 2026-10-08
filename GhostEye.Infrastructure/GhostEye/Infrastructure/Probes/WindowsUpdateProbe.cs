using System;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

public sealed class WindowsUpdateProbe(IRegistryService registry, ILocalizer loc) : IAnalysisProbe
{
	private const string UpdatePolicy = "HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU";

	private const string UxSettings = "HKLM\\SOFTWARE\\Microsoft\\WindowsUpdate\\UX\\Settings";

	public string Id => "win.update";

	public AnalysisGroup Group => AnalysisGroup.Windows;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		bool flag = registry.ReadDword("HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\WindowsUpdate\\AU", "NoAutoUpdate") == 1;
		bool flag2 = DateTime.TryParse(registry.ReadString("HKLM\\SOFTWARE\\Microsoft\\WindowsUpdate\\UX\\Settings", "PauseUpdatesExpiryTime"), out var result) && result > DateTime.UtcNow;
		AnalysisStatus status = ((flag | flag2) ? AnalysisStatus.Warning : AnalysisStatus.Ok);
		string description;
		if (flag)
		{
			description = loc["probe.update.disabled"];
		}
		else
		{
			description = (flag2 ? loc.Format("probe.update.paused", result.ToLocalTime().ToString("d", loc.Culture)) : loc["probe.update.ok"]);
		}
		return Task.FromResult(new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.update.name"],
			Status = status,
			Description = description
		});
	}
}
