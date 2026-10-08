
namespace GhostEye.Core.Models;

public sealed record AnalysisItem
{
	public required string Id { get; init; }

	public required AnalysisGroup Group { get; init; }

	public required string Name { get; init; }

	public required string Description { get; init; }

	public required AnalysisStatus Status { get; init; }

	public string? Measurement { get; init; }

	public string? SuggestedTweakId { get; init; }
}
