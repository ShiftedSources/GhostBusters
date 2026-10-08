using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public interface IServiceBackupService
{
	IReadOnlyList<ServiceBackup> List();

	Task<ServiceBackup> CreateAsync(bool automatic, CancellationToken ct = default(CancellationToken));

	Task<ServiceRestoreResult> RestoreAsync(ServiceBackup backup, CancellationToken ct = default(CancellationToken));

	bool Delete(ServiceBackup backup);
}
