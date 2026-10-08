using System.ServiceProcess;

namespace GhostEye.Infrastructure.Platform;

public sealed record ServiceInfo(string Name, string DisplayName, ServiceControllerStatus Status, ServiceStartupMode StartupMode);
