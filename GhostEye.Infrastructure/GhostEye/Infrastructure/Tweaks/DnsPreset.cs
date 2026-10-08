namespace GhostEye.Infrastructure.Tweaks;

public sealed record DnsPreset(string Id, string Name, string Primary, string Secondary, string[] IPv6, string DohTemplate);
