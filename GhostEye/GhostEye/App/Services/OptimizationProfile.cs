using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GhostEye.App.Services;

public sealed record OptimizationProfile
{
	[JsonPropertyName("format")]
	public string Format { get; init; } = "ghosteye-profile";

	[JsonPropertyName("version")]
	public int Version { get; init; } = 1;

	[JsonPropertyName("name")]
	public string Name { get; init; } = string.Empty;

	[JsonPropertyName("createdAt")]
	public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.Now;

	[JsonPropertyName("tweaks")]
	public IReadOnlyList<string> Tweaks { get; init; } = Array.Empty<string>();

	[JsonPropertyName("services")]
	public IReadOnlyList<string> Services { get; init; } = Array.Empty<string>();

	[JsonPropertyName("dnsPrimary")]
	public string? DnsPrimary { get; init; }

	[JsonPropertyName("dnsSecondary")]
	public string? DnsSecondary { get; init; }

	public const string FormatName = "ghosteye-profile";

	public const int CurrentVersion = 1;
}
