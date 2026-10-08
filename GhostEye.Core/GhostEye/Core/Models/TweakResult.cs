
namespace GhostEye.Core.Models;

public sealed record TweakResult
{
	public required string TweakId { get; init; }

	public required bool Success { get; init; }

	public string Message { get; init; } = string.Empty;

	public string? PreviousValue { get; init; }

	public bool RestartRequired { get; init; }

	public static TweakResult Ok(string tweakId, string? previousValue, string message = "", bool restartRequired = false)
	{
		return new TweakResult
		{
			TweakId = tweakId,
			Success = true,
			PreviousValue = previousValue,
			Message = message,
			RestartRequired = restartRequired
		};
	}

	public static TweakResult Fail(string tweakId, string message)
	{
		return new TweakResult
		{
			TweakId = tweakId,
			Success = false,
			Message = message
		};
	}
}
