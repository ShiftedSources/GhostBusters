namespace GhostEye.Core.Abstractions;

public sealed record TweakApplicability(bool IsApplicable, string? Reason = null)
{
	public static readonly TweakApplicability Applicable = new TweakApplicability(IsApplicable: true);

	public static TweakApplicability No(string reason)
	{
		return new TweakApplicability(IsApplicable: false, reason);
	}
}
