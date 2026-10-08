using System;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Games;

namespace GhostEye.App.ViewModels;

public sealed class FpsEntryViewModel(FrameCaptureEntry entry, FrameCaptureEntry? previous)
{
	public FrameCaptureEntry Entry { get; } = entry;

	public string GameName => Entry.GameName;

	public string DateLabel => Entry.CapturedAt.LocalDateTime.ToString("g", AppServices.Localizer.Culture);

	public string AverageLabel => AppServices.Localizer.Format("fps.value", Math.Round(Entry.Stats.AverageFps));

	public string LowLabel => AppServices.Localizer.Format("fps.low", Math.Round(Entry.Stats.OnePercentLowFps));

	public bool HasDelta => (object)previous != null;

	public string DeltaLabel
	{
		get
		{
			if ((object)previous != null)
			{
				return AppServices.Localizer.Format("fps.delta", Percent(Entry.Stats.AverageFps, previous.Stats.AverageFps), Percent(Entry.Stats.OnePercentLowFps, previous.Stats.OnePercentLowFps));
			}
			return string.Empty;
		}
	}

	public bool IsImprovement
	{
		get
		{
			if ((object)previous != null)
			{
				return Entry.Stats.AverageFps >= previous.Stats.AverageFps;
			}
			return false;
		}
	}

	private static string Percent(double now, double before)
	{
		if (before <= 0.0)
		{
			return "—";
		}
		double num = (now - before) / before * 100.0;
		return ((num >= 0.0) ? "+" : string.Empty) + num.ToString("0.#", AppServices.Localizer.Culture) + "%";
	}
}
