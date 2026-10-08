namespace GhostEye.App.Services;

public sealed record ChangeDetailLine(string Target, string Before, string After)
{
	public bool HasAfter => After.Length > 0;
}
