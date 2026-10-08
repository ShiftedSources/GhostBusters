using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostEye.Core.Models;

public sealed record ChangeSet
{
	public required string Id { get; init; }

	public required string Title { get; init; }

	public required TweakCategory Category { get; init; }

	public required DateTimeOffset AppliedAt { get; init; }

	public required IReadOnlyList<TweakChangeRecord> Changes { get; init; }

	public long? RestorePointSequence { get; init; }

	public IReadOnlyList<string> RegistryBackupFiles { get; init; } = Array.Empty<string>();

	public bool FullyReverted
	{
		get
		{
			if (Changes.Count > 0)
			{
				return Changes.All((TweakChangeRecord c) => c.Reverted);
			}
			return false;
		}
	}
}
