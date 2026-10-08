using System.Collections.Generic;

namespace GhostEye.Infrastructure.Disk;

public sealed record DuplicateGroup(string Hash, long SizeBytes, IReadOnlyList<DuplicateFile> Files)
{
	public long WastedBytes => SizeBytes * (Files.Count - 1);
}
