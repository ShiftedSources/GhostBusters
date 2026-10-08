using System.Collections.Generic;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Platform;

public interface IRegistryService
{
	object? ReadValue(string keyPath, string valueName);

	int? ReadDword(string keyPath, string valueName);

	string? ReadString(string keyPath, string valueName);

	byte[]? ReadBinary(string keyPath, string valueName);

	RegistryValueKind? GetValueKind(string keyPath, string valueName);

	void WriteValue(string keyPath, string valueName, object value, RegistryValueKind kind);

	void WriteDword(string keyPath, string valueName, int value);

	void WriteString(string keyPath, string valueName, string value);

	void DeleteValue(string keyPath, string valueName);

	void DeleteKey(string keyPath);

	bool KeyExists(string keyPath);

	IReadOnlyList<string> GetSubKeyNames(string keyPath);

	IReadOnlyList<string> GetValueNames(string keyPath);
}
