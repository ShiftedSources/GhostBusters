using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

[SupportedOSPlatform("windows")]
public sealed class DnsTweak(IDnsPreferenceProvider preferences, IProcessRunner processRunner, ILocalizer loc) : ITweakAction
{
	internal sealed record AdapterDnsState(string SettingId, string Caption, string[]? Servers, bool FromDhcp, string[]? ServersV6 = null);

	internal sealed record LiveAdapter(string Guid, string Alias, string[] StaticV4, string[] StaticV6, string[] CurrentV4);

	private const string CaptureScript = "[Console]::OutputEncoding=[Text.Encoding]::UTF8;$root='HKLM:\\SYSTEM\\CurrentControlSet\\Services';$domain=[bool](Get-CimInstance Win32_ComputerSystem -ErrorAction SilentlyContinue).PartOfDomain;$list=@(Get-NetIPConfiguration -ErrorAction SilentlyContinue | Where-Object { $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' } | ForEach-Object { $g=[string]$_.NetAdapter.InterfaceGuid; $v4=@(([string](Get-ItemProperty (Join-Path $root ('Tcpip\\Parameters\\Interfaces\\' + $g)) -ErrorAction SilentlyContinue).NameServer) -split '[,\\s]+' | Where-Object { $_ }); $v6=@(([string](Get-ItemProperty (Join-Path $root ('Tcpip6\\Parameters\\Interfaces\\' + $g)) -ErrorAction SilentlyContinue).NameServer) -split '[,\\s]+' | Where-Object { $_ }); $cur=@((Get-DnsClientServerAddress -InterfaceIndex $_.InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue).ServerAddresses); [pscustomobject]@{ G=$g; A=[string]$_.InterfaceAlias; S4=$v4; S6=$v6; C=$cur } });[pscustomobject]@{ D=$domain; L=$list } | ConvertTo-Json -Compress -Depth 4";

	public string Id => "network.dns-optimized";

	public async Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		List<string> list = (await CaptureAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).Item1.SelectMany((LiveAdapter a) => a.StaticV4).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		return (list.Count == 0) ? loc["result.dns.dhcp"] : string.Join(", ", list);
	}

	public async Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		string[] desired = DesiredServers();
		if (desired.Length == 0)
		{
			return false;
		}
		(List<LiveAdapter>, bool) tuple = await CaptureAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		var (adapters, _) = tuple;
		return !tuple.Item2 && IsAppliedTo(adapters, desired[0]);
	}

	public async Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		if (!IsValidIPv4(preferences.PrimaryDns))
		{
			return TweakApplicability.No(loc["applicability.dnsInvalid"]);
		}
		(List<LiveAdapter>, bool) tuple = await CaptureAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		var (list, _) = tuple;
		if (tuple.Item2)
		{
			return TweakApplicability.No(loc["applicability.dnsDomain"]);
		}
		return (list.Count == 0) ? TweakApplicability.No(loc["applicability.dnsNoAdapters"]) : TweakApplicability.Applicable;
	}

	public async Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		string[] desired = DesiredServers();
		if (desired.Length == 0)
		{
			return TweakResult.Fail(Id, loc["result.dns.invalidPrimary"]);
		}
		(List<LiveAdapter>, bool) tuple = await CaptureAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		var (list, _) = tuple;
		if (tuple.Item2)
		{
			return TweakResult.Fail(Id, loc["applicability.dnsDomain"]);
		}
		if (list.Count == 0)
		{
			return TweakResult.Fail(Id, loc["result.dns.noAdapters"]);
		}
		List<AdapterDnsState> states = list.Select((LiveAdapter a) => new AdapterDnsState(a.Guid, a.Alias, (a.StaticV4.Length == 0) ? null : a.StaticV4, a.StaticV4.Length == 0, (a.StaticV6.Length == 0) ? null : a.StaticV6)).ToList();
		string[] servers = desired.Concat(DesiredV6()).ToArray();
		string script = string.Join(";", list.Select((LiveAdapter a) => SetScript(a.Guid, reset: false, servers)));
		string failures = await RunAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false);
		int num = (await CaptureAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).Item1.Count((LiveAdapter a) => a.CurrentV4.FirstOrDefault()?.Equals(desired[0], StringComparison.OrdinalIgnoreCase) ?? false);
		if (num == 0)
		{
			return TweakResult.Fail(Id, failures?.Replace("GHOSTEYE-FAIL", string.Empty, StringComparison.Ordinal).Trim() ?? loc["result.dns.accessDenied"]);
		}
		return TweakResult.Ok(Id, JsonSerializer.Serialize(states), loc.Format("result.dns.applied", string.Join(" / ", desired), num));
	}

	public async Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		List<AdapterDnsState> states;
		try
		{
			states = ((previousValue == null) ? null : JsonSerializer.Deserialize<List<AdapterDnsState>>(previousValue));
		}
		catch (JsonException)
		{
			states = null;
		}
		states = states?.Where((AdapterDnsState s) => Guid.TryParse(s.SettingId, out var _)).ToList();
		if (states == null || states.Count == 0)
		{
			return TweakResult.Fail(Id, loc["result.dns.noPrevious"]);
		}
		string script = string.Join(";", states.Select((AdapterDnsState s) =>
		{
			string[] servers = (s.FromDhcp ? Array.Empty<string>() : (s.Servers ?? Array.Empty<string>())).Concat(s.ServersV6 ?? Array.Empty<string>()).Where(IsValidAddress).ToArray();
			return SetScript(s.SettingId, reset: true, servers);
		}));
		string text = await RunAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false);
		if (text != null && text.Contains("GHOSTEYE-FAIL", StringComparison.Ordinal))
		{
			return TweakResult.Fail(Id, text.Replace("GHOSTEYE-FAIL", string.Empty, StringComparison.Ordinal).Trim());
		}
		return TweakResult.Ok(Id, previousValue, loc.Format("result.dns.reverted", states.Count));
	}

	internal static bool IsAppliedTo(IReadOnlyCollection<LiveAdapter> adapters, string primary)
	{
		if (adapters.Count > 0)
		{
			return adapters.All((LiveAdapter a) => a.CurrentV4.FirstOrDefault()?.Equals(primary, StringComparison.OrdinalIgnoreCase) ?? false);
		}
		return false;
	}

	private static string SetScript(string guid, bool reset, string[] servers)
	{
		string text = Guid.Parse(guid).ToString("B").ToUpperInvariant();
		string text2 = string.Join(",", servers.Select((string s) => $"'{IPAddress.Parse(s.Trim())}'"));
		string text3 = (reset ? "Set-DnsClientServerAddress -InterfaceIndex $a.ifIndex -ResetServerAddresses -ErrorAction Stop;" : string.Empty) + ((servers.Length != 0) ? ("Set-DnsClientServerAddress -InterfaceIndex $a.ifIndex -ServerAddresses @(" + text2 + ") -ErrorAction Stop;") : string.Empty);
		return "try { $a = Get-NetAdapter -IncludeHidden -ErrorAction Stop | Where-Object { $_.InterfaceGuid -eq '" + text + "' } | Select-Object -First 1; if ($a) { " + text3 + " } } catch { Write-Output ('GHOSTEYE-FAIL ' + $_.Exception.Message) }";
	}

	private async Task<string?> RunAsync(string script, CancellationToken ct)
	{
		ProcessResult processResult;
		try
		{
			processResult = await processRunner.RunPowerShellAsync(script + ";Clear-DnsClientCache -ErrorAction SilentlyContinue", ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
		{
			return "GHOSTEYE-FAIL " + ex.Message;
		}
		string text = processResult.Combined.Trim();
		if (text.Contains("Access is denied", StringComparison.OrdinalIgnoreCase) || text.Contains("0x80070005", StringComparison.OrdinalIgnoreCase))
		{
			return "GHOSTEYE-FAIL " + loc["result.dns.accessDenied"];
		}
		return text.Contains("GHOSTEYE-FAIL", StringComparison.Ordinal) ? text : null;
	}

	private async Task<(List<LiveAdapter> Adapters, bool Domain)> CaptureAsync(CancellationToken ct)
	{
		ProcessResult processResult;
		try
		{
			processResult = await processRunner.RunPowerShellAsync("[Console]::OutputEncoding=[Text.Encoding]::UTF8;$root='HKLM:\\SYSTEM\\CurrentControlSet\\Services';$domain=[bool](Get-CimInstance Win32_ComputerSystem -ErrorAction SilentlyContinue).PartOfDomain;$list=@(Get-NetIPConfiguration -ErrorAction SilentlyContinue | Where-Object { $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' } | ForEach-Object { $g=[string]$_.NetAdapter.InterfaceGuid; $v4=@(([string](Get-ItemProperty (Join-Path $root ('Tcpip\\Parameters\\Interfaces\\' + $g)) -ErrorAction SilentlyContinue).NameServer) -split '[,\\s]+' | Where-Object { $_ }); $v6=@(([string](Get-ItemProperty (Join-Path $root ('Tcpip6\\Parameters\\Interfaces\\' + $g)) -ErrorAction SilentlyContinue).NameServer) -split '[,\\s]+' | Where-Object { $_ }); $cur=@((Get-DnsClientServerAddress -InterfaceIndex $_.InterfaceIndex -AddressFamily IPv4 -ErrorAction SilentlyContinue).ServerAddresses); [pscustomobject]@{ G=$g; A=[string]$_.InterfaceAlias; S4=$v4; S6=$v6; C=$cur } });[pscustomobject]@{ D=$domain; L=$list } | ConvertTo-Json -Compress -Depth 4", ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
		{
			return (Adapters: new List<LiveAdapter>(), Domain: false);
		}
		return ParseCapture(processResult.StandardOutput);
	}

	internal static (List<LiveAdapter> Adapters, bool Domain) ParseCapture(string json)
	{
		List<LiveAdapter> list = new List<LiveAdapter>();
		string text = json.Trim();
		if (text.Length == 0)
		{
			return (Adapters: list, Domain: false);
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(text);
			JsonElement rootElement = jsonDocument.RootElement;
			bool item = rootElement.TryGetProperty("D", out var value) && value.ValueKind == JsonValueKind.True;
			if (rootElement.TryGetProperty("L", out var value2))
			{
				List<JsonElement> list2;
				if (value2.ValueKind != JsonValueKind.Array)
				{
					int num = 1;
					list2 = new List<JsonElement>(num);
					CollectionsMarshal.SetCount(list2, num);
					CollectionsMarshal.AsSpan(list2)[0] = value2;
				}
				else
				{
					list2 = value2.EnumerateArray().ToList();
				}
				foreach (JsonElement item2 in list2.Where((JsonElement i) => i.ValueKind == JsonValueKind.Object))
				{
					string text2 = (item2.TryGetProperty("G", out var value3) ? value3.GetString() : null);
					if (Guid.TryParse(text2, out var _))
					{
						list.Add(new LiveAdapter(text2, item2.TryGetProperty("A", out var value4) ? (value4.GetString() ?? "adapter") : "adapter", Strings(item2, "S4").Where(IsValidAddress).ToArray(), Strings(item2, "S6").Where(IsValidAddress).ToArray(), Strings(item2, "C")));
					}
				}
			}
			return (Adapters: list, Domain: item);
		}
		catch (JsonException)
		{
			return (Adapters: list, Domain: false);
		}
	}

	private static string[] Strings(JsonElement item, string name)
	{
		if (!item.TryGetProperty(name, out var value))
		{
			return Array.Empty<string>();
		}
		return value.ValueKind switch
		{
			JsonValueKind.Array => (from v in value.EnumerateArray()
				where v.ValueKind == JsonValueKind.String
				select v.GetString()).ToArray(), 
			JsonValueKind.String => new string[1] { value.GetString() }, 
			_ => Array.Empty<string>(), 
		};
	}

	private string[] DesiredServers()
	{
		if (!IsValidIPv4(preferences.PrimaryDns))
		{
			return Array.Empty<string>();
		}
		if (IsValidIPv4(preferences.SecondaryDns))
		{
			return new string[2]
			{
				preferences.PrimaryDns.Trim(),
				preferences.SecondaryDns.Trim()
			};
		}
		return new string[1] { preferences.PrimaryDns.Trim() };
	}

	private string[] DesiredV6()
	{
		return DnsPresets.FindByAddress(preferences.PrimaryDns)?.IPv6 ?? Array.Empty<string>();
	}

	private static bool IsValidIPv4(string? address)
	{
		if (IPAddress.TryParse(address?.Trim(), out IPAddress address2))
		{
			return address2.AddressFamily == AddressFamily.InterNetwork;
		}
		return false;
	}

	private static bool IsValidAddress(string? address)
	{
		bool flag = IPAddress.TryParse(address?.Trim(), out IPAddress address2);
		if (flag)
		{
			AddressFamily addressFamily = address2.AddressFamily;
			bool flag2 = ((addressFamily == AddressFamily.InterNetwork || addressFamily == AddressFamily.InterNetworkV6) ? true : false);
			flag = flag2;
		}
		return flag;
	}
}
