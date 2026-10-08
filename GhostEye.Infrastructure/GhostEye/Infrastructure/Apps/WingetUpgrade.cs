namespace GhostEye.Infrastructure.Apps;

public sealed record WingetUpgrade(string Name, string Id, string Version, string Available)
{
	public bool IsIdTruncated => Id.EndsWith('…');
}
