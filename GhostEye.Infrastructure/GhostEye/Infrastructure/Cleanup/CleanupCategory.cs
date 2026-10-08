using System.Collections.Generic;

namespace GhostEye.Infrastructure.Cleanup;

public sealed record CleanupCategory
{
	public required string Id { get; init; }

	public required string Name { get; init; }

	public required string Description { get; init; }

	public required long Bytes { get; init; }

	public required int FileCount { get; init; }

	public required IReadOnlyList<string> Roots { get; init; }

	public string? Warning { get; init; }
}
