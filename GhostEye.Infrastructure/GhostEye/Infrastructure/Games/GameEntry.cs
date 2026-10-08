using System;

namespace GhostEye.Infrastructure.Games;

public sealed record GameEntry(string Id, string Name, string Launcher, string InstallPath, bool IsExecutable = false)
{
	public bool Owns(string processPath)
	{
		if (!IsExecutable)
		{
			return processPath.StartsWith(InstallPath.TrimEnd('\\') + "\\", StringComparison.OrdinalIgnoreCase);
		}
		return string.Equals(processPath, InstallPath, StringComparison.OrdinalIgnoreCase);
	}
}
