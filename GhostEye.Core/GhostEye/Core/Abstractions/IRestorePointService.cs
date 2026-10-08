using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Core.Abstractions;

public interface IRestorePointService
{
	Task<long?> CreateAsync(string description, CancellationToken ct = default(CancellationToken));

	Task<bool> IsSystemRestoreEnabledAsync(CancellationToken ct = default(CancellationToken));
}
