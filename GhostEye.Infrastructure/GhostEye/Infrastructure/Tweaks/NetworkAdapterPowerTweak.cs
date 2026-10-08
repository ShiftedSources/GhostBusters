using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class NetworkAdapterPowerTweak : RegistryTweakBase
{
	private const string NetworkClassKey = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}";

	private const int DisablePowerSaving = 24;

	protected override IReadOnlyList<RegistryWrite> Writes => (from key in FindAdapterKeys(Registry)
		select RegistryWrite.Dword(key, "PnPCapabilities", 24)).ToList();

	protected override string SuccessKey => "result.adapterPower";

	public NetworkAdapterPowerTweak(IRegistryService registry, ILocalizer loc)
		: base("network.adapter-power-off", registry, loc)
	{
	}

	public override Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult((FindAdapterKeys(Registry).Count > 0) ? TweakApplicability.Applicable : TweakApplicability.No(Loc["applicability.noNetworkAdapters"]));
	}

	internal static List<string> FindAdapterKeys(IRegistryService registry)
	{
		List<string> list = new List<string>();
		foreach (string subKeyName in registry.GetSubKeyNames("HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}"))
		{
			if (subKeyName.Length == 4 && subKeyName.All(char.IsDigit))
			{
				string text = "HKLM\\SYSTEM\\CurrentControlSet\\Control\\Class\\{4d36e972-e325-11ce-bfc1-08002be10318}\\" + subKeyName;
				string text2 = registry.ReadString(text, "ComponentId");
				if (!string.IsNullOrWhiteSpace(text2) && !IsVirtual(text2))
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	private static bool IsVirtual(string componentId)
	{
		if (!componentId.StartsWith("ROOT\\", StringComparison.OrdinalIgnoreCase) && !componentId.StartsWith("SW\\", StringComparison.OrdinalIgnoreCase) && !componentId.Contains("vwifi", StringComparison.OrdinalIgnoreCase) && !componentId.Contains("vmnet", StringComparison.OrdinalIgnoreCase) && !componentId.Contains("VMS_MP", StringComparison.OrdinalIgnoreCase))
		{
			return componentId.Contains("loopback", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
