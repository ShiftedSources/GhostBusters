using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public interface IProcessRunner
{
	Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken ct = default(CancellationToken));

	Task<ProcessResult> RunPowerShellAsync(string command, CancellationToken ct = default(CancellationToken));
}
