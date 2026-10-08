using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

public sealed class StartupAppsProbe(IStartupManagerService startup, ILocalizer loc) : IAnalysisProbe
{
	public string Id => "win.startup";

	public AnalysisGroup Group => AnalysisGroup.Windows;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		int num = startup.GetEntries().Count((StartupEntry e) => !e.IsDisabled);
		AnalysisStatus analysisStatus = ((num >= 12) ? AnalysisStatus.Critical : ((num >= 6) ? AnalysisStatus.Warning : AnalysisStatus.Ok));
		AnalysisStatus analysisStatus2 = analysisStatus;
		return Task.FromResult(new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.startup.name"],
			Measurement = $"{num}",
			Status = analysisStatus2,
			Description = ((analysisStatus2 == AnalysisStatus.Ok) ? loc["probe.startup.ok"] : loc.Format("probe.startup.many", num)),
			SuggestedTweakId = ((analysisStatus2 == AnalysisStatus.Ok) ? null : "perf.startup-cleanup")
		});
	}
}
