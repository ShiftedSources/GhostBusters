using System;
using System.Collections.Generic;

namespace GhostEye.Core.Models;

public sealed record TweakDefinition
{
	public required string Id { get; init; }

	public required string NameKey { get; init; }

	public required string DescriptionKey { get; init; }

	public required TweakCategory Category { get; init; }

	public required RiskLevel Risk { get; init; }

	public bool RequiresRestart { get; init; }

	public bool AdvancedOnly { get; init; }

	public IReadOnlyList<string> BackupKeys { get; init; } = Array.Empty<string>();
}
