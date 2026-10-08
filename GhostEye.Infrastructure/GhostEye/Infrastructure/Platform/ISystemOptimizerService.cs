using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Presets;

namespace GhostEye.Infrastructure.Platform;

public interface ISystemOptimizerService
{
	IReadOnlyList<OptionalServiceState> GetServices();

	OperationResult SetServiceOptimized(ServiceTweakDefinition definition, bool optimized);

	Task<IReadOnlyList<OptionalTaskState>> GetTasksAsync(CancellationToken ct = default(CancellationToken));

	Task<OperationResult> SetTaskOptimizedAsync(TaskTweakDefinition definition, bool optimized, CancellationToken ct = default(CancellationToken));
}
