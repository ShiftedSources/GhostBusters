using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Models;

namespace GhostEye.Core.Abstractions;

public interface IChangeHistoryStore
{
	Task<IReadOnlyList<ChangeSet>> GetAllAsync(CancellationToken ct = default(CancellationToken));

	Task AppendAsync(ChangeSet changeSet, CancellationToken ct = default(CancellationToken));

	Task UpdateAsync(ChangeSet changeSet, CancellationToken ct = default(CancellationToken));

	Task<bool> DeleteAsync(string changeSetId, CancellationToken ct = default(CancellationToken));

	Task ClearAsync(CancellationToken ct = default(CancellationToken));
}
