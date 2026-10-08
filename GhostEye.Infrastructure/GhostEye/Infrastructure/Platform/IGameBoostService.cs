using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public interface IGameBoostService
{
	bool IsActive { get; }

	BoostState? State { get; }

	IReadOnlyList<BoostApp> GetClosableApps();

	Task<BoostReport> StartAsync(BoostOptions options, CancellationToken ct = default(CancellationToken));

	Task<BoostReport> StopAsync(bool reopenApps, CancellationToken ct = default(CancellationToken));
}
