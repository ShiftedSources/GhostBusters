using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Cleanup;

public interface ICleanupScanner
{
	Task<CleanupResult> ScanAsync(IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));

	Task<CleanupOutcome> CleanAsync(IEnumerable<CleanupCategory> categories, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));
}
