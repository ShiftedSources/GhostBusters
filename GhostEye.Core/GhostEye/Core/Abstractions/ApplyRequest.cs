using System.Collections.Generic;
using GhostEye.Core.Models;

namespace GhostEye.Core.Abstractions;

public sealed record ApplyRequest
{
	public required IReadOnlyList<string> TweakIds { get; init; }

	public required string Title { get; init; }

	public required TweakCategory Category { get; init; }

	public bool CreateRestorePoint { get; init; } = true;

	public bool BackupRegistry { get; init; } = true;
}
