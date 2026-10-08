using GhostEye.Core.Presets;

namespace GhostEye.Infrastructure.Platform;

public sealed record OptionalTaskState(TaskTweakDefinition Definition, bool Exists, bool IsEnabled)
{
	public bool IsOptimized
	{
		get
		{
			if (Exists)
			{
				return !IsEnabled;
			}
			return false;
		}
	}
}
