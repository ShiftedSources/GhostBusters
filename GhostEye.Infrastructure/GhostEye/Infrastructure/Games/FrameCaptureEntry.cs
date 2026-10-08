using System;

namespace GhostEye.Infrastructure.Games;

public sealed record FrameCaptureEntry
{
	public required string Id { get; init; }

	public required string GameName { get; init; }

	public required DateTimeOffset CapturedAt { get; init; }

	public required FrameStats Stats { get; init; }

	public int ActiveTweaks { get; init; }
}
