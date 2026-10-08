using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public interface INetworkToolsService
{
	IReadOnlyList<PingTarget> PingTargets { get; }

	Task<PingResult> PingAsync(PingTarget target, int count = 8, CancellationToken ct = default(CancellationToken));

	Task<SpeedResult> SpeedTestAsync(IProgress<string>? progress, CancellationToken ct = default(CancellationToken));

	Task<OperationResult> ResetNetworkAsync(IProgress<string>? progress, CancellationToken ct = default(CancellationToken));
}
