using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

[SupportedOSPlatform("windows")]
public sealed class ServicesProbe(IServiceManagerService services, ILocalizer loc) : IAnalysisProbe
{
	private static readonly (string Name, string PurposeKey)[] OptionalServices = new (string, string)[8]
	{
		("DiagTrack", "probe.service.diagTrack"),
		("dmwappushservice", "probe.service.dmwappush"),
		("RetailDemo", "probe.service.retailDemo"),
		("MapsBroker", "probe.service.mapsBroker"),
		("Fax", "probe.service.fax"),
		("XblAuthManager", "probe.service.xblAuth"),
		("XblGameSave", "probe.service.xblSave"),
		("WSearch", "probe.service.search")
	};

	public string Id => "win.services";

	public AnalysisGroup Group => AnalysisGroup.Windows;

	public Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.Run(() =>
		{
			List<(string, string, ServiceInfo)> list = OptionalServices.Select(((string Name, string PurposeKey) entry) => (Name: entry.Name, Purpose: loc[entry.PurposeKey], Info: services.GetService(entry.Name))).Where(((string Name, string Purpose, ServiceInfo Info) x) =>
			{
				ServiceInfo item = x.Info;
				return (object)item != null && item.Status == ServiceControllerStatus.Running;
			}).ToList();
			AnalysisStatus status = ((list.Count > 0) ? AnalysisStatus.Warning : AnalysisStatus.Ok);
			string text = string.Join(", ", list.Take(3).Select(((string Name, string Purpose, ServiceInfo Info) r) => r.Purpose));
			if (list.Count > 3)
			{
				text += ", …";
			}
			return new AnalysisItem
			{
				Id = Id,
				Group = Group,
				Name = loc["probe.services.name"],
				Measurement = $"{list.Count}",
				Status = status,
				Description = ((list.Count == 0) ? loc["probe.services.none"] : loc.Format("probe.services.some", list.Count, text))
			};
		}, ct);
	}
}
