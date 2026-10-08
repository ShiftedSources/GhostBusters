using System;

namespace GhostEye.Infrastructure.Platform;

public static class StartupCommand
{
	public static string? ExtractExecutable(string? command)
	{
		if (string.IsNullOrWhiteSpace(command))
		{
			return null;
		}
		string text = Environment.ExpandEnvironmentVariables(command.Trim());
		if (text.StartsWith('"'))
		{
			int num = text.IndexOf('"', 1);
			if (num <= 1)
			{
				return null;
			}
			return text.Substring(1, num - 1);
		}
		int num2 = text.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
		if (num2 > 0)
		{
			return text.Substring(0, num2 + 4);
		}
		int num3 = text.IndexOf(' ');
		if (num3 <= 0)
		{
			return text;
		}
		return text.Substring(0, num3);
	}
}
