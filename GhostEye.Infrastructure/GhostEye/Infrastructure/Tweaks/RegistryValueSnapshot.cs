
namespace GhostEye.Infrastructure.Tweaks;

public sealed record RegistryValueSnapshot
{
	public required string Key { get; init; }

	public required string Name { get; init; }

	public string? Kind { get; init; }

	public string? Value { get; init; }

	public bool Existed => Kind != null;
}
