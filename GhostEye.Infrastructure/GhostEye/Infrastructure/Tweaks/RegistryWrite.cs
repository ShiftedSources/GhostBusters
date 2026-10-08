using Microsoft.Win32;

namespace GhostEye.Infrastructure.Tweaks;

public sealed record RegistryWrite(string Key, string Name, object DesiredValue, RegistryValueKind Kind)
{
	public static RegistryWrite Dword(string key, string name, int value)
	{
		return new RegistryWrite(key, name, value, RegistryValueKind.DWord);
	}

	public static RegistryWrite Text(string key, string name, string value)
	{
		return new RegistryWrite(key, name, value, RegistryValueKind.String);
	}
}
