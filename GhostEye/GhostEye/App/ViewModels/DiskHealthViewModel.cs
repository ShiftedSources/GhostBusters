using System.Collections.Generic;
using System.Linq;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Health;

namespace GhostEye.App.ViewModels;

public sealed class DiskHealthViewModel(DiskHealth disk)
{
	public DiskHealth Disk { get; } = disk;

	public string Name => Disk.Name;

	public string Kind => string.Join(" · ", new string[3]
	{
		Disk.MediaType,
		Disk.BusType,
		GhostFormat.Bytes(Disk.SizeBytes)
	}.Where((string s) => !string.IsNullOrWhiteSpace(s) && s != "Unspecified"));

	public HealthLevel Level => Disk.Level;

	public string LevelLabel => AppServices.Localizer[$"health.level.{Disk.Level}"];

	public string Details
	{
		get
		{
			ILocalizer localizer = AppServices.Localizer;
			if (Disk.PredictsFailure)
			{
				return localizer["health.disk.predictFailure"];
			}
			if (!Disk.HasCounters)
			{
				return localizer[AdminGate.IsElevated ? "health.disk.notReported" : "health.disk.noCounters"];
			}
			List<string> list = new List<string>();
			int? temperatureC = Disk.TemperatureC;
			if (temperatureC.HasValue)
			{
				int valueOrDefault = temperatureC.GetValueOrDefault();
				list.Add(localizer.Format("health.disk.temperature", valueOrDefault));
			}
			temperatureC = Disk.WearPercent;
			if (temperatureC.HasValue)
			{
				int valueOrDefault2 = temperatureC.GetValueOrDefault();
				list.Add(localizer.Format("health.disk.wear", 100 - valueOrDefault2));
			}
			long? powerOnHours = Disk.PowerOnHours;
			if (powerOnHours.HasValue)
			{
				long valueOrDefault3 = powerOnHours.GetValueOrDefault();
				list.Add(localizer.Format("health.disk.hours", valueOrDefault3.ToString("N0", localizer.Culture)));
			}
			powerOnHours = Disk.UncorrectedReadErrors;
			if (powerOnHours.HasValue)
			{
				long valueOrDefault4 = powerOnHours.GetValueOrDefault();
				if (valueOrDefault4 > 0)
				{
					list.Add(localizer.Format("health.disk.errors", valueOrDefault4));
				}
			}
			return string.Join(" · ", list);
		}
	}
}
