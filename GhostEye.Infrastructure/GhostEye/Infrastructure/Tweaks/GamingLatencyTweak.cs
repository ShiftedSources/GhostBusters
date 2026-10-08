using System;
using System.Collections.Generic;
using System.Linq;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class GamingLatencyTweak : RegistryTweakBase
{
	private const string SystemProfile = "HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile";

	private const string TcpInterfaces = "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces";

	protected override IReadOnlyList<RegistryWrite> Writes
	{
		get
		{
			List<RegistryWrite> list = new List<RegistryWrite>
			{
				RegistryWrite.Dword("HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", "NetworkThrottlingIndex", -1),
				RegistryWrite.Dword("HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile", "SystemResponsiveness", 10)
			};
			foreach (string item in PhysicalInterfaceKeys())
			{
				list.Add(RegistryWrite.Dword(item, "TcpAckFrequency", 1));
				list.Add(RegistryWrite.Dword(item, "TCPNoDelay", 1));
			}
			return list;
		}
	}

	protected override string SuccessKey => "result.gamingLatency";

	protected override bool RequiresRestart => true;

	public GamingLatencyTweak(IRegistryService registry, ILocalizer loc)
		: base("network.gaming-latency", registry, loc)
	{
	}

	private IEnumerable<string> PhysicalInterfaceKeys()
	{
		HashSet<string> existing = Registry.GetSubKeyNames("HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces").ToHashSet(StringComparer.OrdinalIgnoreCase);
		foreach (string item in NetworkAdapterPowerTweak.FindAdapterKeys(Registry))
		{
			string text = Registry.ReadString(item, "NetCfgInstanceId");
			if (!string.IsNullOrWhiteSpace(text) && existing.Contains(text))
			{
				yield return "HKLM\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" + text;
			}
		}
	}
}
