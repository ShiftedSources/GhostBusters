using System;
using System.Threading.Tasks;
using GhostEye.App.Services;
using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class GamingViewModel : CategoryPageViewModel
{
	public BenchmarkViewModel Benchmark { get; } = new BenchmarkViewModel();

	public GameBoostViewModel Boost { get; } = new GameBoostViewModel();

	public AutoBoostViewModel AutoBoost { get; } = new AutoBoostViewModel();

	public FpsCaptureViewModel Fps { get; } = new FpsCaptureViewModel();

	public GamingViewModel()
		: base(TweakCategory.Gaming, "gaming.category", "Icon.Gaming", "history.title.gaming")
	{
		BackgroundAgent.BoostChanged += (object? _, EventArgs _) =>
		{
			Boost.Refresh();
		};
	}

	public override Task ActivatedAsync()
	{
		Boost.Refresh();
		AutoBoost.Refresh();
		Fps.RefreshTargets();
		return base.ActivatedAsync();
	}

	public override void OnLanguageChanged()
	{
		base.OnLanguageChanged();
		Benchmark.OnLanguageChanged();
		Boost.OnLanguageChanged();
		AutoBoost.OnLanguageChanged();
		Fps.OnLanguageChanged();
	}
}
