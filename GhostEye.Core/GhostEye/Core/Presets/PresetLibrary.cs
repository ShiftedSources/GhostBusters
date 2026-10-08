using System.Collections.Generic;
using System.Linq;
using GhostEye.Core.Models;

namespace GhostEye.Core.Presets;

public static class PresetLibrary
{
	private static readonly IReadOnlyList<Preset> AllPresets = new List<Preset>
	{
		new Preset
		{
			Id = "balanced",
			IconKey = "Icon.Preset.Balanced",
			NameKey = "preset.balanced.name",
			DescriptionKey = "preset.balanced.desc",
			AccentColor = "#5FD3A3",
			TweakIds = new string[5] { "perf.power-plan-high", "perf.startup-cleanup", "perf.storage-optimize", "gaming.game-mode", "network.adapter-power-off" }
		},
		new Preset
		{
			Id = "performance",
			IconKey = "Icon.Preset.Performance",
			NameKey = "preset.performance.name",
			DescriptionKey = "preset.performance.desc",
			AccentColor = "#8FD3FF",
			TweakIds = new string[9] { "perf.power-plan-high", "perf.foreground-priority", "perf.visual-effects", "perf.startup-cleanup", "perf.storage-optimize", "gaming.game-mode", "gaming.gpu-scheduling", "network.adapter-power-off", "network.tcp-optimize" }
		},
		new Preset
		{
			Id = "extreme",
			IconKey = "Icon.Preset.Extreme",
			NameKey = "preset.extreme.name",
			DescriptionKey = "preset.extreme.desc",
			AccentColor = "#E2685F",
			RequiresConfirmation = true,
			TweakIds = new string[20]
			{
				"perf.power-plan-high", "perf.foreground-priority", "perf.visual-effects", "perf.startup-cleanup", "perf.storage-optimize", "gaming.game-mode", "gaming.disable-game-bar", "gaming.gpu-scheduling", "gaming.fullscreen-opt", "gaming.block-background-recording",
				"gaming.mouse-latency", "network.tcp-optimize", "network.adapter-power-off", "network.gaming-latency", "network.delivery-optimization", "privacy.telemetry", "privacy.advertising-id", "privacy.activity-history", "privacy.windows-tips", "privacy.auto-feedback"
			}
		},
		new Preset
		{
			Id = "gaming",
			IconKey = "Icon.Preset.Gaming",
			NameKey = "preset.gaming.name",
			DescriptionKey = "preset.gaming.desc",
			AccentColor = "#C78FFF",
			TweakIds = new string[10] { "perf.power-plan-high", "perf.foreground-priority", "gaming.game-mode", "gaming.disable-game-bar", "gaming.gpu-scheduling", "gaming.fullscreen-opt", "gaming.block-background-recording", "gaming.mouse-latency", "network.adapter-power-off", "network.gaming-latency" }
		},
		new Preset
		{
			Id = "laptop",
			IconKey = "Icon.Preset.Laptop",
			NameKey = "preset.laptop.name",
			DescriptionKey = "preset.laptop.desc",
			AccentColor = "#E8B158",
			TweakIds = new string[5] { "perf.visual-effects", "perf.startup-cleanup", "perf.storage-optimize", "privacy.telemetry", "privacy.windows-tips" }
		},
		new Preset
		{
			Id = "safe",
			IconKey = "Icon.Preset.Safe",
			NameKey = "preset.safe.name",
			DescriptionKey = "preset.safe.desc",
			AccentColor = "#8A94A6",
			TweakIds = new string[5] { "perf.startup-cleanup", "perf.storage-optimize", "gaming.game-mode", "privacy.advertising-id", "privacy.windows-tips" }
		}
	};

	public static IReadOnlyList<Preset> All => AllPresets;

	public static Preset? Find(string id)
	{
		return AllPresets.FirstOrDefault((Preset p) => p.Id == id);
	}
}
