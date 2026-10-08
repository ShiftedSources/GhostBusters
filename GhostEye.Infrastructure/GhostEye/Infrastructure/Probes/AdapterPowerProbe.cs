using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

public sealed class AdapterPowerProbe(IRegistryService registry, ILocalizer loc) : IAnalysisProbe
{
	private const string NetworkClassKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}";

	public string Id => "net.adapter-power";

	public AnalysisGroup Group => AnalysisGroup.Network;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		int num = 0;
		int num2 = 0;
		foreach (string subKeyName in registry.GetSubKeyNames("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}"))
		{
			if (subKeyName.Length != 4 || !subKeyName.All(char.IsDigit))
			{
				continue;
			}
			string keyPath = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}\\" + subKeyName;
			string text = registry.ReadString(keyPath, "ComponentId");
			if (!string.IsNullOrWhiteSpace(text) && !text.StartsWith("ROOT\\", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("SW\\", StringComparison.OrdinalIgnoreCase))
			{
				num++;
				int? num3 = registry.ReadDword(keyPath, "PnPCapabilities");
				if (!num3.HasValue || (num3.Value & 0x18) == 0)
				{
					num2++;
				}
			}
		}
		if (num == 0)
		{
			return Task.FromResult(new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.adapterPower.name"],
				Description = loc["probe.adapterPower.none"],
				Status = AnalysisStatus.Ok
			});
		}
		AnalysisStatus analysisStatus = ((num2 > 0) ? AnalysisStatus.Warning : AnalysisStatus.Ok);
		return Task.FromResult(new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.adapterPower.name"],
			Measurement = $"{num2}/{num}",
			Status = analysisStatus,
			Description = ((analysisStatus == AnalysisStatus.Ok) ? loc["probe.adapterPower.ok"] : loc.Format("probe.adapterPower.some", num2, num)),
			SuggestedTweakId = ((analysisStatus == AnalysisStatus.Ok) ? null : "network.adapter-power-off")
		});
	}
}
