using System.Globalization;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class ServiceBackupItemViewModel(ServiceBackup backup)
{
	public ServiceBackup Backup { get; } = backup;

	public string DateLabel => Backup.CreatedAt.ToString("g", CultureInfo.CurrentCulture);

	public string KindLabel => AppServices.Localizer[Backup.IsAutomatic ? "services.backup.auto" : "services.backup.manual"];

	public string Meta => AppServices.Localizer.Format("services.backup.meta", Backup.Services.Count, Backup.Tasks.Count);
}
