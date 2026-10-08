using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

public sealed class BackgroundAppsProbe(IRegistryService registry, ILocalizer loc) : IAnalysisProbe
{
	private const string BackgroundAccess = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\BackgroundAccessApplications";

	private const string SearchSettings = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Search";

	public string Id => "win.background-apps";

	public AnalysisGroup Group => AnalysisGroup.Windows;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		int count = registry.GetSubKeyNames("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\BackgroundAccessApplications").Count;
		bool flag = registry.ReadDword("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Search", "BackgroundAppGlobalToggle") != 0;
		AnalysisStatus analysisStatus = ((count >= 10) ? AnalysisStatus.Warning : AnalysisStatus.Ok);
		string text = (flag ? loc["probe.background.globalToggle"] : loc["probe.background.period"]);
		return Task.FromResult(new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.background.name"],
			Measurement = $"{count}",
			Status = analysisStatus,
			Description = ((analysisStatus == AnalysisStatus.Ok) ? loc["probe.background.ok"] : loc.Format("probe.background.many", count, text))
		});
	}
}
