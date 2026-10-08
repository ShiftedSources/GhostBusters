using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class DnsBenchmarkRowViewModel(DnsBenchmarkResult result, bool isBest) : ObservableObject
{
	public DnsBenchmarkResult Result { get; } = result;

	public bool IsBest { get; } = isBest;

	public string Name => Result.Name;

	public string Time
	{
		get
		{
			double? medianMs = Result.MedianMs;
			if (medianMs.HasValue)
			{
				double valueOrDefault = medianMs.GetValueOrDefault();
				return Math.Round(valueOrDefault).ToString(AppServices.Localizer.Culture) + " ms";
			}
			return AppServices.Localizer["network.ping.noReply"];
		}
	}

	public string Detail
	{
		get
		{
			if (Result.LossPercent <= 0)
			{
				if (!Result.IsCurrent)
				{
					return string.Empty;
				}
				return AppServices.Localizer["network.dns.benchmark.inUse"];
			}
			return AppServices.Localizer.Format("network.dns.benchmark.loss", Result.LossPercent);
		}
	}

	public bool CanUse
	{
		get
		{
			if (Result.MedianMs.HasValue)
			{
				return !Result.IsCurrent;
			}
			return false;
		}
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Time");
		OnPropertyChanged("Detail");
	}
}
