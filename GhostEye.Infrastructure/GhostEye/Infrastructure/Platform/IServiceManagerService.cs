using System.Collections.Generic;

namespace GhostEye.Infrastructure.Platform;

public interface IServiceManagerService
{
	IReadOnlyList<ServiceInfo> GetServices();

	ServiceInfo? GetService(string serviceName);

	ServiceStartupMode? GetStartupMode(string serviceName);

	bool TrySetStartupMode(string serviceName, ServiceStartupMode mode, out string error);

	bool TryStop(string serviceName, out string error);

	bool TryStart(string serviceName, out string error);
}
