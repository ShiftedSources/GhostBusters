using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public interface IStartupManagerService
{
	IReadOnlyList<StartupEntry> GetEntries();

	Task<IReadOnlyList<StartupEntry>> GetAllEntriesAsync(CancellationToken ct = default(CancellationToken));

	Task<StartupOperationResult> SetEnabledAsync(StartupEntry entry, bool enabled, CancellationToken ct = default(CancellationToken));

	StartupOperationResult Remove(StartupEntry entry);

	StartupOperationResult Restore(StartupEntry entry);

	bool TryRestore(string name, StartupScope scope, out string error);
}
