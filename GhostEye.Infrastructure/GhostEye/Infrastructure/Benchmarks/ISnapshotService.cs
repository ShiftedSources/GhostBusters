using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Benchmarks;

public interface ISnapshotService
{
	Task<SystemSnapshot> CaptureAsync(CancellationToken ct = default(CancellationToken));

	SystemSnapshot? LoadBaseline();

	void SaveBaseline(SystemSnapshot snapshot);

	void ClearBaseline();
}
