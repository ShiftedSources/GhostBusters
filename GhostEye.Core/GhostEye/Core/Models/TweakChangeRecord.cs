using System;

namespace GhostEye.Core.Models;

public sealed record TweakChangeRecord
{
	public required string TweakId { get; init; }

	public required string TweakName { get; init; }

	public string? PreviousValue { get; init; }

	public bool Reverted { get; init; }

	public DateTimeOffset? RevertedAt { get; init; }

	public string? Before { get; init; }

	public string? After { get; init; }
}
