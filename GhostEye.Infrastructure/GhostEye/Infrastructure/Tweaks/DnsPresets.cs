using System.Collections.Generic;
using System.Linq;

namespace GhostEye.Infrastructure.Tweaks;

public static class DnsPresets
{
	public static IReadOnlyList<DnsPreset> All { get; } = new _003C_003Ez__ReadOnlyArray<DnsPreset>(new DnsPreset[5]
	{
		new DnsPreset("cloudflare", "Cloudflare", "1.1.1.1", "1.0.0.1", new string[2] { "2606:4700:4700::1111", "2606:4700:4700::1001" }, "https://cloudflare-dns.com/dns-query"),
		new DnsPreset("google", "Google", "8.8.8.8", "8.8.4.4", new string[2] { "2001:4860:4860::8888", "2001:4860:4860::8844" }, "https://dns.google/dns-query"),
		new DnsPreset("quad9", "Quad9", "9.9.9.9", "149.112.112.112", new string[2] { "2620:fe::fe", "2620:fe::9" }, "https://dns.quad9.net/dns-query"),
		new DnsPreset("adguard", "AdGuard", "94.140.14.14", "94.140.15.15", new string[2] { "2a10:50c0::ad1:ff", "2a10:50c0::ad2:ff" }, "https://dns.adguard-dns.com/dns-query"),
		new DnsPreset("opendns", "OpenDNS", "208.67.222.222", "208.67.220.220", new string[2] { "2620:119:35::35", "2620:119:53::53" }, "https://doh.opendns.com/dns-query")
	});

	public static DnsPreset? FindByAddress(string? address)
	{
		string trimmed = address?.Trim();
		if (!string.IsNullOrEmpty(trimmed))
		{
			return All.FirstOrDefault((DnsPreset p) => p.Primary == trimmed || p.Secondary == trimmed);
		}
		return null;
	}
}
