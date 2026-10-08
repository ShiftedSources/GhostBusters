using System.Collections.Generic;

namespace GhostEye.App.Services;

public sealed record ProfileApplyReport(int TweaksApplied, int TweaksFailed, int ServicesApplied, int ServicesFailed, IReadOnlyList<string> Errors)
{
	public bool Success
	{
		get
		{
			if (TweaksFailed == 0)
			{
				return ServicesFailed == 0;
			}
			return false;
		}
	}
}
