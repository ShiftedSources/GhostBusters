using System.ServiceProcess;
using GhostEye.Core.Presets;

namespace GhostEye.Infrastructure.Platform;

public sealed record OptionalServiceState(ServiceTweakDefinition Definition, bool IsInstalled, string DisplayName, ServiceControllerStatus? Status, ServiceStartupMode? StartupMode)
{
	public bool IsOptimized
	{
		get
		{
			ServiceStartupMode? startupMode = StartupMode;
			ServiceStartupMode valueOrDefault = default;
			int num;
			if (startupMode.HasValue)
			{
				valueOrDefault = startupMode.GetValueOrDefault();
				num = 1;
			}
			else
			{
				num = 0;
			}
			bool flag = (byte)num != 0;
			if (flag)
			{
				bool flag2;
				if (Definition.Disable)
				{
					flag2 = valueOrDefault == ServiceStartupMode.Disabled;
				}
				else
				{
					bool flag3 = (uint)(valueOrDefault - 3) <= 1u;
					flag2 = flag3;
				}
				flag = flag2;
			}
			return flag;
		}
	}
}
