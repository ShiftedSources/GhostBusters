using System.Security.Principal;

namespace GhostEye.Infrastructure.Platform;

public static class Elevation
{
	public static bool IsElevated { get; } = Detect();

	private static bool Detect()
	{
		using WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent();
		return new WindowsPrincipal(ntIdentity).IsInRole(WindowsBuiltInRole.Administrator);
	}
}
