using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Win32;

namespace GhostEye.Infrastructure.Tweaks;

public static class RegistrySnapshotCodec
{
	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = false
	};

	public static string Encode(IEnumerable<RegistryValueSnapshot> snapshots)
	{
		return JsonSerializer.Serialize(snapshots.ToList(), Options);
	}

	public static IReadOnlyList<RegistryValueSnapshot>? TryDecode(string? payload)
	{
		if (string.IsNullOrWhiteSpace(payload))
		{
			return null;
		}
		try
		{
			return JsonSerializer.Deserialize<List<RegistryValueSnapshot>>(payload, Options);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	public static string? EncodeValue(object? value)
	{
		if (value != null)
		{
			if (!(value is byte[] inArray))
			{
				if (value is string[] value2)
				{
					return string.Join('\n', value2);
				}
				return value.ToString();
			}
			return Convert.ToBase64String(inArray);
		}
		return null;
	}

	public static object DecodeValue(string encoded, RegistryValueKind kind)
	{
		return kind switch
		{
			RegistryValueKind.DWord => (object)int.Parse(encoded), 
			RegistryValueKind.QWord => long.Parse(encoded), 
			RegistryValueKind.Binary => Convert.FromBase64String(encoded), 
			RegistryValueKind.MultiString => encoded.Split('\n'), 
			_ => encoded, 
		};
	}

	public static RegistryValueKind ParseKind(string? kind)
	{
		if (!Enum.TryParse<RegistryValueKind>(kind, out var result))
		{
			return RegistryValueKind.String;
		}
		return result;
	}
}
