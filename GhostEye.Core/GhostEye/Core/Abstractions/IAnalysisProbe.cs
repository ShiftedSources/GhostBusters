using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Models;

namespace GhostEye.Core.Abstractions;

public interface IAnalysisProbe
{
	string Id { get; }

	AnalysisGroup Group { get; }

	Task<AnalysisItem> RunAsync(CancellationToken ct = default(CancellationToken));
}
