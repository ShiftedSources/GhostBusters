using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using GhostEye.Core.Localization;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Platform;

public sealed class RegistryService : IRegistryService
{
	public static (RegistryKey Root, string SubPath) ParsePath(string fullPath)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(fullPath, "fullPath");
		int num = fullPath.IndexOf('\\');
		string text = ((num < 0) ? fullPath : fullPath.Substring(0, num));
		string item = ((num < 0) ? string.Empty : fullPath.Substring(num + 1));
		RegistryKey item2;
		switch (text.ToUpperInvariant())
		{
		case "HKLM":
		case "HKEY_LOCAL_MACHINE":
			item2 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
			break;
		case "HKLM32":
			item2 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
			break;
		case "HKCU":
		case "HKEY_CURRENT_USER":
			item2 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
			break;
		case "HKCR":
		case "HKEY_CLASSES_ROOT":
			item2 = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
			break;
		case "HKU":
		case "HKEY_USERS":
			item2 = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Registry64);
			break;
		default:
			throw new ArgumentException("Unrecognised registry hive: '" + text + "'.", "fullPath");
		}
		return (Root: item2, SubPath: item);
	}

	public object? ReadValue(string keyPath, string valueName)
	{
		var (registryKey, name) = ParsePath(keyPath);
		using (registryKey)
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(name, writable: false);
			return registryKey2?.GetValue(valueName, null);
		}
	}

	public int? ReadDword(string keyPath, string valueName)
	{
		object obj = ReadValue(keyPath, valueName);
		if (!(obj is int value))
		{
			if (!(obj is uint value2))
			{
				if (obj is long num)
				{
					return (int)num;
				}
				return null;
			}
			return (int)value2;
		}
		return value;
	}

	public string? ReadString(string keyPath, string valueName)
	{
		return ReadValue(keyPath, valueName)?.ToString();
	}

	public byte[]? ReadBinary(string keyPath, string valueName)
	{
		return ReadValue(keyPath, valueName) as byte[];
	}

	public RegistryValueKind? GetValueKind(string keyPath, string valueName)
	{
		var (registryKey, name) = ParsePath(keyPath);
		using (registryKey)
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(name, writable: false);
			if (registryKey2 == null)
			{
				return null;
			}
			try
			{
				return registryKey2.GetValueKind(valueName);
			}
			catch (IOException)
			{
				return null;
			}
		}
	}

	public void WriteValue(string keyPath, string valueName, object value, RegistryValueKind kind)
	{
		var (registryKey, subkey) = ParsePath(keyPath);
		using (registryKey)
		{
			using RegistryKey registryKey2 = registryKey.CreateSubKey(subkey, writable: true) ?? throw new UnauthorizedAccessException("Could not open '" + keyPath + "' for writing.");
			registryKey2.SetValue(valueName, value, kind);
		}
	}

	public void WriteDword(string keyPath, string valueName, int value)
	{
		WriteValue(keyPath, valueName, value, RegistryValueKind.DWord);
	}

	public void WriteString(string keyPath, string valueName, string value)
	{
		WriteValue(keyPath, valueName, value, RegistryValueKind.String);
	}

	public void DeleteValue(string keyPath, string valueName)
	{
		var (registryKey, name) = ParsePath(keyPath);
		using (registryKey)
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(name, writable: true);
			registryKey2?.DeleteValue(valueName, throwOnMissingValue: false);
		}
	}

	public void DeleteKey(string keyPath)
	{
		var (registryKey, subkey) = ParsePath(keyPath);
		using (registryKey)
		{
			registryKey.DeleteSubKeyTree(subkey, throwOnMissingSubKey: false);
		}
	}

	public bool KeyExists(string keyPath)
	{
		var (registryKey, name) = ParsePath(keyPath);
		using (registryKey)
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(name, writable: false);
			return registryKey2 != null;
		}
	}

	public IReadOnlyList<string> GetSubKeyNames(string keyPath)
	{
		var (registryKey, name) = ParsePath(keyPath);
		using (registryKey)
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(name, writable: false);
			return registryKey2?.GetSubKeyNames() ?? Array.Empty<string>();
		}
	}

	public IReadOnlyList<string> GetValueNames(string keyPath)
	{
		var (registryKey, name) = ParsePath(keyPath);
		using (registryKey)
		{
			using RegistryKey registryKey2 = registryKey.OpenSubKey(name, writable: false);
			return registryKey2?.GetValueNames() ?? Array.Empty<string>();
		}
	}

	public static bool TryRun(ILocalizer loc, Action operation, out string error)
	{
		try
		{
			operation();
			error = string.Empty;
			return true;
		}
		catch (UnauthorizedAccessException)
		{
			error = loc["result.registry.accessDenied"];
			return false;
		}
		catch (SecurityException)
		{
			error = loc["result.registry.insufficient"];
			return false;
		}
	}
}
