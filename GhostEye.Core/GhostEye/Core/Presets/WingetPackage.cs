using System.Text.RegularExpressions;

namespace GhostEye.Core.Presets;

public sealed record WingetPackage(string Id, string Name, string Group, string? InstalledPattern = null)
{
	public bool MatchesInstalledName(string displayName)
	{
		if (InstalledPattern != null)
		{
			return Regex.IsMatch(displayName, InstalledPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
		}
		return false;
	}
}
