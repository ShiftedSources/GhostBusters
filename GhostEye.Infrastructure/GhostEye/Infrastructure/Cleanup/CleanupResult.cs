using System.Collections.Generic;
using System.Linq;

namespace GhostEye.Infrastructure.Cleanup;

public sealed record CleanupResult
{
	public required IReadOnlyList<CleanupCategory> Categories { get; init; }

	public long TotalBytes => Categories.Sum((CleanupCategory c) => c.Bytes);
}
