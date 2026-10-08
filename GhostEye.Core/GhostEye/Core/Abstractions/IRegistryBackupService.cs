using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Core.Abstractions;

public interface IRegistryBackupService
{
	Task<IReadOnlyList<string>> ExportAsync(IEnumerable<string> registryKeys, string label, CancellationToken ct = default(CancellationToken));

	Task<bool> ImportAsync(string regFilePath, CancellationToken ct = default(CancellationToken));
}
