using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public interface IPerformanceToolsService
{
	MemoryStatus GetMemoryStatus();

	Task<MemoryCleanResult> FreeMemoryAsync(CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyList<ProcessUsage>> GetHeavyProcessesAsync(int count, CancellationToken ct = default(CancellationToken));

	OperationResult EndProcess(int processId);

	Task<OperationResult> FlushDnsAsync(CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyList<LargeFile>> FindLargeFilesAsync(long minimumBytes, int maxResults, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));
}
