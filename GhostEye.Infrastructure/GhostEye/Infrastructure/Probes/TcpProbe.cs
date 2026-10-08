using System;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Tweaks;

namespace GhostEye.Infrastructure.Probes;

public sealed class TcpProbe(TcpAutotuningTweak tweak, ILocalizer loc) : IAnalysisProbe
{
	public string Id => "net.tcp";

	public AnalysisGroup Group => AnalysisGroup.Network;

	public async Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		string text = await tweak.ReadCurrentValueAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		if (text == null)
		{
			return new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.tcp.name"],
				Description = loc["probe.tcp.unreadable"],
				Status = AnalysisStatus.Warning
			};
		}
		bool flag = text.Equals("normal", StringComparison.OrdinalIgnoreCase);
		return new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.tcp.name"],
			Measurement = text,
			Status = ((!flag) ? AnalysisStatus.Warning : AnalysisStatus.Ok),
			Description = (flag ? loc["probe.tcp.ok"] : loc.Format("probe.tcp.wrong", text)),
			SuggestedTweakId = (flag ? null : "network.tcp-optimize")
		};
	}
}
