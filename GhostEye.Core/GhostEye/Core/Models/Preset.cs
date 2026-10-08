using System.Collections.Generic;

namespace GhostEye.Core.Models;

public sealed record Preset
{
	public required string Id { get; init; }

	public required string NameKey { get; init; }

	public required string DescriptionKey { get; init; }

	public required string AccentColor { get; init; }

	public required string IconKey { get; init; }

	public required IReadOnlyList<string> TweakIds { get; init; }

	public bool RequiresConfirmation { get; init; }
}
