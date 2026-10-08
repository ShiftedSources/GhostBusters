using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Probes;

[SupportedOSPlatform("windows")]
public static class GpuInfo
{
	private const string DisplayClassKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}";

	private static readonly string[] VirtualAdapters = new string[9] { "Microsoft Basic Display", "Microsoft Remote Display", "RDP", "Citrix", "VMware SVGA", "VirtualBox", "Parsec", "DameWare", "Mirror Driver" };

	private static readonly string[] DiscreteMarkers = new string[8] { "RTX", "GTX", "Quadro", "Titan", "Radeon RX", "Radeon Pro", "FirePro", "Arc A" };

	public static GpuAdapter? GetPrimary(IRegistryService registry)
	{
		return (from a in GetAll(registry)
			orderby a.MemoryBytes descending, a.IsDiscrete descending
			select a).FirstOrDefault();
	}

	public static IReadOnlyList<GpuAdapter> GetAll(IRegistryService registry)
	{
		Dictionary<string, long> dictionary = ReadMemorySizes(registry);
		List<GpuAdapter> list = new List<GpuAdapter>();
		try
		{
			using ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, DriverDate, PNPDeviceID FROM Win32_VideoController");
			foreach (ManagementBaseObject item in managementObjectSearcher.Get())
			{
				using (item)
				{
					string name = (item["Name"] as string)?.Trim();
					if (!string.IsNullOrWhiteSpace(name) && !IsVirtual(name) && ((item["PNPDeviceID"] as string) ?? string.Empty).StartsWith("PCI\\", StringComparison.OrdinalIgnoreCase))
					{
						list.Add(new GpuAdapter
						{
							Name = name,
							ShortName = ShortenName(name),
							DriverVersion = (item["DriverVersion"] as string),
							DriverDate = ParseWmiDate(item["DriverDate"] as string),
							MemoryBytes = dictionary.GetValueOrDefault(name, 0L),
							IsDiscrete = DiscreteMarkers.Any((string m) => name.Contains(m, StringComparison.OrdinalIgnoreCase))
						});
					}
				}
			}
		}
		catch (ManagementException)
		{
		}
		return list;
	}

	private static Dictionary<string, long> ReadMemorySizes(IRegistryService registry)
	{
		Dictionary<string, long> dictionary = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
		foreach (string subKeyName in registry.GetSubKeyNames("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}"))
		{
			if (subKeyName.Length != 4 || !subKeyName.All(char.IsDigit))
			{
				continue;
			}
			string keyPath = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e968-e325-11ce-bfc1-08002be10318}\\" + subKeyName;
			string text = registry.ReadString(keyPath, "DriverDesc");
			if (!string.IsNullOrWhiteSpace(text))
			{
				object obj = registry.ReadValue(keyPath, "HardwareInformation.qwMemorySize");
				long num2;
				if (obj is long num)
				{
					num2 = num;
				}
				else if (obj is int num3)
				{
					num2 = num3;
				}
				else
				{
					num2 = ((!(obj is byte[] array) || array.Length != 8) ? 0 : BitConverter.ToInt64(array, 0));
				}
				long num4 = num2;
				if (num4 > 0)
				{
					dictionary[text] = num4;
				}
			}
		}
		return dictionary;
	}

	public static string ShortenName(string? fullName)
	{
		if (string.IsNullOrWhiteSpace(fullName))
		{
			return "—";
		}
		string text = fullName.Trim();
		Match match = NvidiaModel().Match(text);
		if (match.Success)
		{
			return Compose(match.Groups["series"].Value.ToUpperInvariant(), match.Groups["model"].Value, match.Groups["suffix"].Value);
		}
		Match match2 = AmdModel().Match(text);
		if (match2.Success)
		{
			return Compose("RX", match2.Groups["model"].Value, match2.Groups["suffix"].Value);
		}
		Match match3 = ArcModel().Match(text);
		if (match3.Success)
		{
			return "Arc " + match3.Groups["model"].Value.ToUpperInvariant();
		}
		Match match4 = IntelIntegrated().Match(text);
		if (match4.Success)
		{
			string value = match4.Groups["family"].Value;
			string value2 = match4.Groups["model"].Value;
			string text2 = (value.Equals("Iris", StringComparison.OrdinalIgnoreCase) ? "Iris Xe" : (value.ToUpperInvariant() + " Graphics"));
			if (!string.IsNullOrWhiteSpace(value2))
			{
				return text2 + " " + value2;
			}
			return text2;
		}
		return Cleanup(text);
	}

	private static string Compose(string series, string model, string suffix)
	{
		string value = suffix.ToUpperInvariant() switch
		{
			"TI" => "Ti", 
			"SUPER" => "Super", 
			"XT" => "XT", 
			"XTX" => "XTX", 
			"GRE" => "GRE", 
			"M" => "M", 
			_ => string.Empty, 
		};
		if (!string.IsNullOrEmpty(value))
		{
			return $"{series} {model} {value}";
		}
		return series + " " + model;
	}

	private static string Cleanup(string name)
	{
		string[] array = new string[8] { "(R)", "(TM)", "NVIDIA", "AMD", "Intel", "Corporation", "Graphics Family", "Series" }.Aggregate(name, (string current, string token) => current.Replace(token, " ", StringComparison.OrdinalIgnoreCase)).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (array.Length != 0)
		{
			return string.Join(' ', array.Take(3));
		}
		return name;
	}

	private static bool IsVirtual(string name)
	{
		return VirtualAdapters.Any((string v) => name.Contains(v, StringComparison.OrdinalIgnoreCase));
	}

	private static DateTime? ParseWmiDate(string? value)
	{
		if (string.IsNullOrWhiteSpace(value) || value.Length < 8)
		{
			return null;
		}
		if (!DateTime.TryParseExact(value.Substring(0, 8), "yyyyMMdd", null, DateTimeStyles.None, out var result))
		{
			return null;
		}
		return result;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex NvidiaModel()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__NvidiaModel_5.Instance;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex AmdModel()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AmdModel_6.Instance;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex ArcModel()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__ArcModel_7.Instance;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex IntelIntegrated()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__IntelIntegrated_8.Instance;
	}
}
