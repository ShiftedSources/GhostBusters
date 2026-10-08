using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

[SupportedOSPlatform("windows")]
public sealed class NicBufferProbe(IRegistryService registry, ILocalizer loc) : IAnalysisProbe
{
	private const string NetworkClassKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}";

	public string Id => "net.nic-buffer";

	public AnalysisGroup Group => AnalysisGroup.Network;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		List<string> list = new List<string>();
		foreach (string subKeyName in registry.GetSubKeyNames("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}"))
		{
			if (subKeyName.Length == 4 && subKeyName.All(char.IsDigit))
			{
				string keyPath = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}\\" + subKeyName;
				string text = registry.ReadString(keyPath, "DriverDesc");
				string text2 = registry.ReadString(keyPath, "*ReceiveBuffers") ?? registry.ReadString(keyPath, "ReceiveBuffers");
				if (text != null && text2 != null)
				{
					list.Add(text + ": " + text2);
				}
			}
		}
		return Task.FromResult(new AnalysisItem
		{
			Id = Id,
			Group = Group,
			Name = loc["probe.nicBuffer.name"],
			Measurement = ((list.Count > 0) ? list.Count.ToString() : loc["status.none"]),
			Status = AnalysisStatus.Ok,
			Description = ((list.Count == 0) ? loc["probe.nicBuffer.none"] : loc.Format("probe.nicBuffer.some", string.Join(" · ", list)))
		});
	}
}
