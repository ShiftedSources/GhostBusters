using GhostEye.App.Services;
using GhostEye.Infrastructure.Health;

namespace GhostEye.App.ViewModels;

public sealed class DriverViewModel(DriverInfo driver)
{
	public DriverInfo Driver { get; } = driver;

	public string Name => Driver.DeviceName;

	public string ClassLabel => AppServices.Localizer["health.driver.class." + Driver.DeviceClass];

	public string Detail => AppServices.Localizer.Format("health.driver.detail", string.IsNullOrWhiteSpace(Driver.Manufacturer) ? "—" : Driver.Manufacturer, Driver.Version, Driver.Date?.ToString("d", AppServices.Localizer.Culture) ?? "—");

	public bool IsOld => Driver.IsOld;

	public bool HasDownload => Driver.DownloadUrl != null;
}
