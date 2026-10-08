using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Health;

public interface IHealthService
{
	Task<IReadOnlyList<DiskHealth>> GetDisksAsync(CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyList<SecurityCheck>> GetSecurityAsync(CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyList<DriverInfo>> GetDriversAsync(CancellationToken ct = default(CancellationToken));

	Task<RepairResult> RepairAsync(RepairTool tool, IProgress<double>? percent, IProgress<string>? lines, CancellationToken ct = default(CancellationToken));
}
