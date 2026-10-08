using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Apps;

public interface IInstalledAppsService
{
	Task<IReadOnlyList<InstalledApp>> GetDesktopAppsAsync(CancellationToken ct = default(CancellationToken));

	Task<OperationResult> UninstallAsync(InstalledApp app, CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyList<StoreApp>> GetBloatwareAsync(CancellationToken ct = default(CancellationToken));

	Task<OperationResult> RemoveStoreAppAsync(StoreApp app, CancellationToken ct = default(CancellationToken));
}
