
namespace GhostEye.Infrastructure.Cleanup;

public sealed record CleanupOutcome
{
	public required long FreedBytes { get; init; }

	public required int DeletedFiles { get; init; }

	public required int SkippedFiles { get; init; }
}
