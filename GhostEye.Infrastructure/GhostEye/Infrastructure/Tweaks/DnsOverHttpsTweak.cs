using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
public sealed class DnsOverHttpsTweak(IDnsPreferenceProvider preferences, IProcessRunner processRunner, ILocalizer loc) : ITweakAction
{
	private sealed record DohRecord(string[] CreatedKeys, string[] AddedServers, Dictionary<string, long?> ChangedFlags);

	private const string ScopeScript = "$base='HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\InterfaceSpecificParameters';$gs=@(Get-NetIPConfiguration -ErrorAction SilentlyContinue | Where-Object { $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' } | ForEach-Object { [string]$_.NetAdapter.InterfaceGuid });";

	public string Id => "network.dns-doh";

	private static bool IsWindows11 => Environment.OSVersion.Version.Build >= 22000;

	private DnsPreset? Preset => DnsPresets.FindByAddress(preferences.PrimaryDns);

	public async Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return (await IsAppliedAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).ToString();
	}

	public Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		if (!IsWindows11)
		{
			return Task.FromResult(TweakApplicability.No(loc["applicability.dohWindows11"]));
		}
		return Task.FromResult(((object)Preset == null) ? TweakApplicability.No(loc["applicability.dohPreset"]) : TweakApplicability.Applicable);
	}

	public async Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		if (IsWindows11)
		{
			DnsPreset preset = Preset;
			if ((object)preset != null)
			{
				string script = "$base='HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\InterfaceSpecificParameters';$gs=@(Get-NetIPConfiguration -ErrorAction SilentlyContinue | Where-Object { $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' } | ForEach-Object { [string]$_.NetAdapter.InterfaceGuid });$ok = $gs.Count -gt 0; foreach ($g in $gs) { $k = Join-Path $base ($g + '\\DohInterfaceSettings\\Doh\\" + Address(preset.Primary) + "'); if ((Get-ItemProperty -Path $k -Name DohFlags -ErrorAction SilentlyContinue).DohFlags -ne 1) { $ok = $false } }; if (-not (Get-DnsClientDohServerAddress -ServerAddress '" + Address(preset.Primary) + "' -ErrorAction SilentlyContinue)) { $ok = $false }; Write-Output ([string]$ok)";
				return (await RunAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false)).Trim().EndsWith("True", StringComparison.OrdinalIgnoreCase);
			}
		}
		return false;
	}

	public async Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		if (!IsWindows11)
		{
			return TweakResult.Fail(Id, loc["applicability.dohWindows11"]);
		}
		DnsPreset preset = Preset;
		if ((object)preset == null)
		{
			return TweakResult.Fail(Id, loc["applicability.dohPreset"]);
		}
		string[] source = new string[2] { preset.Primary, preset.Secondary }.Concat(preset.IPv6).Select(Address).ToArray();
		string text = string.Join(",", source.Select((string s) => "'" + s + "'"));
		string script = "[Console]::OutputEncoding=[Text.Encoding]::UTF8;$base='HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\InterfaceSpecificParameters';$gs=@(Get-NetIPConfiguration -ErrorAction SilentlyContinue | Where-Object { $_.IPv4DefaultGateway -and $_.NetAdapter.Status -eq 'Up' } | ForEach-Object { [string]$_.NetAdapter.InterfaceGuid });$made=@(); $added=@(); $changed=@{};try {foreach ($s in @(" + text + ")) { if (-not (Get-DnsClientDohServerAddress -ServerAddress $s -ErrorAction SilentlyContinue)) { Add-DnsClientDohServerAddress -ServerAddress $s -DohTemplate '" + Template(preset) + "' -AllowFallbackToUdp $false -AutoUpgrade $false -ErrorAction Stop | Out-Null; $added += $s } };foreach ($g in $gs) { foreach ($s in @(" + text + ")) { $fam = if ($s.Contains(':')) { 'Doh6' } else { 'Doh' }; $k = Join-Path $base ($g + '\\DohInterfaceSettings\\' + $fam + '\\' + $s); if (Test-Path $k) { $changed[$k] = (Get-ItemProperty -Path $k -Name DohFlags -ErrorAction SilentlyContinue).DohFlags } else { New-Item -Path $k -Force | Out-Null; $made += $k }; New-ItemProperty -Path $k -Name DohFlags -Value 1 -PropertyType QWord -Force | Out-Null } };Clear-DnsClientCache -ErrorAction SilentlyContinue;} catch { Write-Output ('GHOSTEYE-FAIL ' + $_.Exception.Message) };Write-Output ('GHOSTEYE-JSON ' + ([pscustomobject]@{ CreatedKeys=@($made); AddedServers=@($added); ChangedFlags=$changed } | ConvertTo-Json -Compress -Depth 4))";
		string output = await RunAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false);
		DohRecord dohRecord = ParseRecord(output);
		if (output.Contains("GHOSTEYE-FAIL", StringComparison.Ordinal))
		{
			if ((object)dohRecord != null)
			{
				await RevertAsync(JsonSerializer.Serialize(dohRecord), ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			string message = output.Substring(output.IndexOf("GHOSTEYE-FAIL", StringComparison.Ordinal) + 13).Split('\n')[0].Trim();
			return TweakResult.Fail(Id, message);
		}
		if ((object)dohRecord == null)
		{
			return TweakResult.Fail(Id, loc["result.dns.accessDenied"]);
		}
		return TweakResult.Ok(Id, JsonSerializer.Serialize(dohRecord), loc.Format("result.doh.applied", preset.Name));
	}

	public async Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		DohRecord dohRecord;
		try
		{
			dohRecord = ((previousValue == null) ? null : JsonSerializer.Deserialize<DohRecord>(previousValue));
		}
		catch (JsonException)
		{
			dohRecord = null;
		}
		if ((object)dohRecord == null)
		{
			return TweakResult.Fail(Id, loc["result.noSnapshot"]);
		}
		IEnumerable<string> values = from k in dohRecord.CreatedKeys.Where(IsDohKey)
			select "'" + k + "'";
		IEnumerable<string> source = dohRecord.ChangedFlags.Where((KeyValuePair<string, long?> p) => IsDohKey(p.Key)).Select((KeyValuePair<string, long?> p) =>
		{
			long? value = p.Value;
			if (value.HasValue)
			{
				long valueOrDefault = value.GetValueOrDefault();
				return $"New-ItemProperty -Path '{p.Key}' -Name DohFlags -Value {valueOrDefault} -PropertyType QWord -Force | Out-Null";
			}
			return "Remove-ItemProperty -Path '" + p.Key + "' -Name DohFlags -ErrorAction SilentlyContinue";
		});
		IEnumerable<string> values2 = from s in dohRecord.AddedServers
			where IPAddress.TryParse(s, out IPAddress _)
			select $"'{IPAddress.Parse(s)}'";
		string script = string.Concat("try {foreach ($k in @(", string.Join(",", values), ")) { Remove-Item -Path $k -Recurse -Force -ErrorAction SilentlyContinue };", string.Concat(source.Select((string r) => r + ";")), "foreach ($s in @(", string.Join(",", values2), ")) { Remove-DnsClientDohServerAddress -ServerAddress $s -ErrorAction SilentlyContinue };Clear-DnsClientCache -ErrorAction SilentlyContinue} catch { Write-Output ('GHOSTEYE-FAIL ' + $_.Exception.Message) }");
		string text = await RunAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false);
		return text.Contains("GHOSTEYE-FAIL", StringComparison.Ordinal) ? TweakResult.Fail(Id, text.Replace("GHOSTEYE-FAIL", string.Empty, StringComparison.Ordinal).Trim()) : TweakResult.Ok(Id, previousValue, loc["result.reverted"]);
	}

	private static bool IsDohKey(string key)
	{
		if (key.StartsWith("HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Dnscache\\InterfaceSpecificParameters\\", StringComparison.OrdinalIgnoreCase) && key.Contains("\\DohInterfaceSettings\\", StringComparison.OrdinalIgnoreCase) && !key.Contains('\'') && !key.Contains('`'))
		{
			return !key.Contains('$');
		}
		return false;
	}

	private static DohRecord? ParseRecord(string output)
	{
		string text = (from l in output.Split('\n')
			select l.Trim()).LastOrDefault((string l) => l.StartsWith("GHOSTEYE-JSON ", StringComparison.Ordinal));
		if (text == null)
		{
			return null;
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(text.Substring("GHOSTEYE-JSON ".Length));
			JsonElement rootElement = jsonDocument.RootElement;
			Dictionary<string, long?> dictionary = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase);
			if (rootElement.TryGetProperty("ChangedFlags", out var value) && value.ValueKind == JsonValueKind.Object)
			{
				foreach (JsonProperty item in value.EnumerateObject())
				{
					dictionary[item.Name] = ((item.Value.ValueKind == JsonValueKind.Number) ? new long?(item.Value.GetInt64()) : ((long?)null));
				}
			}
			return new DohRecord(Strings(rootElement, "CreatedKeys"), Strings(rootElement, "AddedServers"), dictionary);
		}
		catch (JsonException)
		{
			return null;
		}
		static string[] Strings(JsonElement e, string name)
		{
			if (e.TryGetProperty(name, out var value2))
			{
				if (value2.ValueKind != JsonValueKind.Array)
				{
					if (value2.ValueKind == JsonValueKind.String)
					{
						return new string[1] { value2.GetString() };
					}
					return Array.Empty<string>();
				}
				return (from x in value2.EnumerateArray()
					select x.GetString() ?? string.Empty into x
					where x.Length > 0
					select x).ToArray();
			}
			return Array.Empty<string>();
		}
	}

	private async Task<string> RunAsync(string script, CancellationToken ct)
	{
		try
		{
			return (await processRunner.RunPowerShellAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false)).Combined;
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
		{
			return "GHOSTEYE-FAIL " + ex.Message;
		}
	}

	private static string Address(string address)
	{
		return IPAddress.Parse(address).ToString();
	}

	private static string Template(DnsPreset preset)
	{
		if (!Uri.TryCreate(preset.DohTemplate, UriKind.Absolute, out Uri result) || !(result.Scheme == Uri.UriSchemeHttps) || preset.DohTemplate.Contains('\''))
		{
			throw new InvalidOperationException("Invalid DoH template.");
		}
		return preset.DohTemplate;
	}
}
