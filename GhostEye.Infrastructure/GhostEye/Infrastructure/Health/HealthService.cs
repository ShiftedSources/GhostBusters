using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Health;

public sealed class HealthService(IProcessRunner processRunner, ILocalizer loc) : IHealthService
{
	private static readonly string[] DriverClasses = new string[7] { "DISPLAY", "NET", "MEDIA", "BLUETOOTH", "HDC", "SCSIADAPTER", "USB" };

	public async Task<IReadOnlyList<DiskHealth>> GetDisksAsync(CancellationToken ct = default(CancellationToken))
	{
		return (await RunJsonAsync("[Console]::OutputEncoding=[Text.Encoding]::UTF8;$fp = @(Get-CimInstance -Namespace root/wmi -ClassName MSStorageDriver_FailurePredictStatus -ErrorAction SilentlyContinue);$dd = @(Get-CimInstance -ClassName Win32_DiskDrive -ErrorAction SilentlyContinue);@(Get-PhysicalDisk | ForEach-Object { $r = $_ | Get-StorageReliabilityCounter -ErrorAction SilentlyContinue; $id = $_.DeviceId; $d = $dd | Where-Object { [string]$_.Index -eq [string]$id } | Select-Object -First 1; $f = $false; if ($d) { if ($d.Status -eq 'Pred Fail') { $f = $true }; foreach ($p in $fp) { if ($p.PredictFailure -and $p.InstanceName.StartsWith([string]$d.PNPDeviceID, [StringComparison]::OrdinalIgnoreCase)) { $f = $true } } }; [pscustomobject]@{ N = $_.FriendlyName; M = [string]$_.MediaType; B = [string]$_.BusType; S = [long]$_.Size; H = [string]$_.HealthStatus; T = $r.Temperature; W = $r.Wear; P = $r.PowerOnHours; E = $r.ReadErrorsUncorrected; F = $f } }) | ConvertTo-Json -Compress", ct).ConfigureAwait(continueOnCapturedContext: false)).Select((JsonElement e) =>
		{
			long? temperature = Long(e, "T");
			long? wear = Long(e, "W");
			long? powerOnHours = Long(e, "P");
			return new DiskHealth
			{
				Name = (Str(e, "N") ?? "Disk"),
				MediaType = (Str(e, "M") ?? string.Empty),
				BusType = (Str(e, "B") ?? string.Empty),
				SizeBytes = Long(e, "S").GetValueOrDefault(),
				HealthStatus = (Str(e, "H") ?? string.Empty),
				TemperatureC = temperature is > 0 and < 150 ? (int?)temperature.Value : null,
				WearPercent = wear is >= 0 and <= 100 && powerOnHours.HasValue ? (int?)wear.Value : null,
				PowerOnHours = powerOnHours is > 0 ? powerOnHours : null,
				UncorrectedReadErrors = Long(e, "E"),
				PredictsFailure = e.TryGetProperty("F", out var value) && value.ValueKind == JsonValueKind.True
			};
		}).ToList();
	}

	public async Task<IReadOnlyList<SecurityCheck>> GetSecurityAsync(CancellationToken ct = default(CancellationToken))
	{
		List<JsonElement> list = await RunJsonAsync("[Console]::OutputEncoding=[Text.Encoding]::UTF8;$av = @(Get-CimInstance -Namespace root/SecurityCenter2 -ClassName AntivirusProduct -ErrorAction SilentlyContinue | ForEach-Object { [pscustomobject]@{ N = $_.displayName; S = [long]$_.productState } });$mp = Get-MpComputerStatus -ErrorAction SilentlyContinue;$fw = @(Get-NetFirewallProfile -ErrorAction SilentlyContinue | ForEach-Object { [pscustomobject]@{ N = [string]$_.Name; E = [bool]$_.Enabled } });$bl = $null; try { $v = Get-CimInstance -Namespace root/cimv2/Security/MicrosoftVolumeEncryption -ClassName Win32_EncryptableVolume -ErrorAction Stop | Where-Object { $_.DriveLetter -eq $env:SystemDrive }; if ($v) { $bl = [int]$v.ProtectionStatus } } catch { $bl = -1 };$sb = $null; try { $sb = [bool](Confirm-SecureBootUEFI -ErrorAction Stop) } catch { };$reboot = Test-Path 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\RebootRequired';$hf = Get-HotFix -ErrorAction SilentlyContinue | Where-Object { $_.InstalledOn } | Sort-Object InstalledOn -Descending | Select-Object -First 1;$uac = (Get-ItemProperty 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System' -ErrorAction SilentlyContinue).EnableLUA;[pscustomobject]@{ AV = $av; MpRt = $mp.RealTimeProtectionEnabled; MpAge = $mp.AntivirusSignatureAge; FW = $fw; BL = $bl; SB = $sb; RB = $reboot; HF = if ($hf) { $hf.InstalledOn.ToString('yyyy-MM-dd') } else { $null }; UAC = $uac } | ConvertTo-Json -Compress -Depth 4", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (list.Count == 0)
		{
			return Array.Empty<SecurityCheck>();
		}
		JsonElement e = list[0];
		List<SecurityCheck> list2 = new List<SecurityCheck>();
		List<(string, long)> list3 = ((e.TryGetProperty("AV", out var value) && value.ValueKind == JsonValueKind.Array) ? (from p in value.EnumerateArray()
			select (Name: Str(p, "N") ?? "?", State: Long(p, "S").GetValueOrDefault())).ToList() : new List<(string, long)>());
		List<(string, long)> list4 = list3.Where(((string Name, long State) p) => IsAntivirusOn(p.State)).ToList();
		list2.Add((list4.Count > 0) ? new SecurityCheck("antivirus", (!list4.Any(((string Name, long State) p) => IsAntivirusUpToDate(p.State))) ? HealthLevel.Warning : HealthLevel.Good, string.Join(", ", list4.Select(((string Name, long State) p) => p.Name).Distinct())) : new SecurityCheck("antivirus", HealthLevel.Bad, (list3.Count > 0) ? string.Join(", ", list3.Select(((string Name, long State) p) => p.Name)) : "—"));
		List<(string, bool)> list5 = ((e.TryGetProperty("FW", out var value2) && value2.ValueKind == JsonValueKind.Array) ? (from p in value2.EnumerateArray()
			select (Name: Str(p, "N") ?? "?", On: p.TryGetProperty("E", out var value5) && value5.ValueKind == JsonValueKind.True)).ToList() : new List<(string, bool)>());
		List<SecurityCheck> list6 = list2;
		SecurityCheck item;
		if (list5.Count != 0)
		{
			item = (list5.All(((string Name, bool On) p) => p.On) ? new SecurityCheck("firewall", HealthLevel.Good, string.Join(", ", list5.Select(((string Name, bool On) p) => p.Name))) : new SecurityCheck("firewall", HealthLevel.Bad, string.Join(", ", list5.Where(((string Name, bool On) p) => !p.On).Select(((string Name, bool On) p) => p.Name))));
		}
		else
		{
			item = new SecurityCheck("firewall", HealthLevel.Unknown, "—");
		}
		list6.Add(item);
		string text = Str(e, "HF");
		DateTime? dateTime = ((text != null && DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)) ? new DateTime?(result) : ((DateTime?)null));
		bool flag = e.TryGetProperty("RB", out var value3) && value3.ValueKind == JsonValueKind.True;
		int num;
		if (dateTime.HasValue)
		{
			DateTime now = DateTime.Now;
			DateTime? dateTime2 = dateTime;
			if (!(now - dateTime2 > TimeSpan.FromDays(60.0)))
			{
				now = DateTime.Now;
				dateTime2 = dateTime;
				num = (((now - dateTime2 > TimeSpan.FromDays(35.0)) | flag) ? 1 : 0);
			}
			else
			{
				num = 2;
			}
		}
		else
		{
			num = 3;
		}
		HealthLevel level = (HealthLevel)num;
		list2.Add(new SecurityCheck("updates", level, (dateTime?.ToString("d", loc.Culture) ?? "—") + (flag ? (" · " + loc["health.security.rebootPending"]) : string.Empty)));
		long? num2 = Long(e, "BL");
		List<SecurityCheck> list7 = list2;
		list7.Add(num2 switch
		{
			1L => new SecurityCheck("bitlocker", HealthLevel.Good, loc["health.security.on"]), 
			0L => new SecurityCheck("bitlocker", HealthLevel.Warning, loc["health.security.off"]), 
			_ => new SecurityCheck("bitlocker", HealthLevel.Unknown, loc["health.security.needsAdmin"]), 
		});
		list7 = list2;
		bool flag2 = e.TryGetProperty("SB", out var value4);
		if (flag2)
		{
			JsonValueKind valueKind = value4.ValueKind;
			bool flag3 = valueKind - 5 <= JsonValueKind.Object;
			flag2 = flag3;
		}
		list7.Add(flag2 ? new SecurityCheck("secureboot", (value4.ValueKind != JsonValueKind.True) ? HealthLevel.Warning : HealthLevel.Good, loc[(value4.ValueKind == JsonValueKind.True) ? "health.security.on" : "health.security.off"]) : new SecurityCheck("secureboot", HealthLevel.Unknown, loc["health.security.needsAdmin"]));
		long? num3 = Long(e, "UAC");
		list2.Add(new SecurityCheck("uac", (num3 == 0) ? HealthLevel.Bad : HealthLevel.Good, loc[(num3 == 0) ? "health.security.off" : "health.security.on"]));
		return list2;
	}

	public static bool IsAntivirusOn(long state)
	{
		long num = (state >> 8) & 0xFF;
		if ((ulong)(num - 16) <= 1uL)
		{
			return true;
		}
		return false;
	}

	public static bool IsAntivirusUpToDate(long state)
	{
		return (state & 0xFF) == 0;
	}

	public async Task<IReadOnlyList<DriverInfo>> GetDriversAsync(CancellationToken ct = default(CancellationToken))
	{
		string text = string.Join(",", DriverClasses.Select((string c) => "'" + c + "'"));
		string script = "[Console]::OutputEncoding=[Text.Encoding]::UTF8;@(Get-CimInstance Win32_PnPSignedDriver | Where-Object { $_.DeviceName -and @(" + text + ") -contains ([string]$_.DeviceClass).ToUpper() } | ForEach-Object { [pscustomobject]@{ N = $_.DeviceName; C = [string]$_.DeviceClass; M = $_.Manufacturer; V = $_.DriverVersion; D = if ($_.DriverDate) { $_.DriverDate.ToString('yyyy-MM-dd') } else { $null } } }) | ConvertTo-Json -Compress";
		return (from d in (await RunJsonAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false)).Select((JsonElement e) =>
			{
				string text2 = Str(e, "M") ?? string.Empty;
				string text3 = Str(e, "N") ?? string.Empty;
				DateTime result;
				return new DriverInfo(text3, (Str(e, "C") ?? string.Empty).ToUpperInvariant(), text2, Str(e, "V") ?? string.Empty, DateTime.TryParseExact(Str(e, "D"), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? new DateTime?(result) : ((DateTime?)null), DownloadUrlFor(text2 + " " + text3));
			})
			group d by (DeviceName: d.DeviceName, Version: d.Version) into g
			select g.First() into d
			orderby ClassOrder(d.DeviceClass)
			select d).ThenBy((DriverInfo d) => d.DeviceName, StringComparer.CurrentCultureIgnoreCase).ToList();
	}

	public static string? DownloadUrlFor(string text)
	{
		if (text.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase))
		{
			return "https://www.nvidia.com/Download/index.aspx";
		}
		if (text.Contains("AMD", StringComparison.OrdinalIgnoreCase) || text.Contains("Advanced Micro Devices", StringComparison.OrdinalIgnoreCase) || text.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
		{
			return "https://www.amd.com/en/support/download/drivers.html";
		}
		if (text.Contains("Intel", StringComparison.OrdinalIgnoreCase))
		{
			return "https://www.intel.com/content/www/us/en/support/detect.html";
		}
		if (text.Contains("Realtek", StringComparison.OrdinalIgnoreCase))
		{
			return "https://www.realtek.com/Download/Index?menu_id=297";
		}
		return null;
	}

	private static int ClassOrder(string deviceClass)
	{
		int num = Array.IndexOf(DriverClasses, deviceClass);
		if (num < 0)
		{
			return 99;
		}
		return num;
	}

	public async Task<RepairResult> RepairAsync(RepairTool tool, IProgress<double>? percent, IProgress<string>? lines, CancellationToken ct = default(CancellationToken))
	{
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
		Encoding encoding;
		string arguments;
		string fileName;
		if (tool != RepairTool.SystemFileChecker)
		{
			string text = Path.Combine(folderPath, "dism.exe");
			Encoding uTF = Encoding.UTF8;
			encoding = uTF;
			arguments = "/Online /Cleanup-Image /RestoreHealth";
			fileName = text;
		}
		else
		{
			string text2 = Path.Combine(folderPath, "sfc.exe");
			Encoding uTF = Encoding.Unicode;
			encoding = uTF;
			arguments = "/scannow";
			fileName = text2;
		}
		Progress<string> lines2 = new Progress<string>((string line) =>
		{
			Match match = PercentPattern().Match(line);
			if (match.Success && double.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				percent?.Report(Math.Clamp(result, 0.0, 100.0));
			}
			else
			{
				lines?.Report(line);
			}
		});
		ProcessResult processResult = await StreamingProcess.RunAsync(fileName, arguments, lines2, encoding, ct).ConfigureAwait(continueOnCapturedContext: false);
		return new RepairResult(Summary: string.Join(values: (from l in processResult.Combined.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			where !PercentPattern().IsMatch(l)
			select l).TakeLast(3), separator: Environment.NewLine), Success: processResult.Success);
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex PercentPattern()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__PercentPattern_4.Instance;
	}

	private async Task<List<JsonElement>> RunJsonAsync(string script, CancellationToken ct)
	{
		ProcessResult processResult;
		try
		{
			processResult = await processRunner.RunPowerShellAsync(script, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is InvalidOperationException) ? true : false)
		{
			return new List<JsonElement>();
		}
		string text = processResult.StandardOutput.Trim();
		if (text.Length == 0)
		{
			return new List<JsonElement>();
		}
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(text);
			return (jsonDocument.RootElement.ValueKind == JsonValueKind.Array) ? (from e in jsonDocument.RootElement.EnumerateArray()
				select e.Clone()).ToList() : new List<JsonElement>(1) { jsonDocument.RootElement.Clone() };
		}
		catch (JsonException)
		{
			return new List<JsonElement>();
		}
	}

	private static string? Str(JsonElement e, string name)
	{
		if (!e.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
		{
			return null;
		}
		return value.GetString();
	}

	private static long? Long(JsonElement e, string name)
	{
		if (!e.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number || !value.TryGetInt64(out var value2))
		{
			return null;
		}
		return value2;
	}
}
