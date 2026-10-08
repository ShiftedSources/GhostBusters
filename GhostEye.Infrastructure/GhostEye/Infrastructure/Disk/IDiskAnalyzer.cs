using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Disk;

public interface IDiskAnalyzer
{
	IReadOnlyList<DriveSummary> GetDrives();

	Task<IReadOnlyList<FolderUsage>> MeasureAsync(string folder, IProgress<string>? progress, CancellationToken ct = default(CancellationToken));

	Task<IReadOnlyList<DuplicateGroup>> FindDuplicatesAsync(IReadOnlyList<string> roots, long minimumBytes, IProgress<string>? progress, CancellationToken ct = default(CancellationToken));
}
