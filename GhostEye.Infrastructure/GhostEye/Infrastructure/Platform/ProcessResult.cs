using System;

namespace GhostEye.Infrastructure.Platform;

public sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError)
{
	public bool Success => ExitCode == 0;

	public string Combined
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(StandardError))
			{
				return StandardOutput + Environment.NewLine + StandardError;
			}
			return StandardOutput;
		}
	}
}
