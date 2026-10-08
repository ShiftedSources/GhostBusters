using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Benchmarks;

namespace GhostEye.App.ViewModels;

public sealed class BenchmarkViewModel : ObservableObject, ILocalizedViewModel
{
	private readonly ISnapshotService _snapshots;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? captureBaselineCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? compareCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? clearBaselineCommand;

	public ObservableCollection<BenchmarkRow> Rows { get; } = new ObservableCollection<BenchmarkRow>();

	public SystemSnapshot? Baseline { get; private set; }

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsBusy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsBusy);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsBusy);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string StatusMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.StatusMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.StatusMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string EmptyMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.EmptyMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.EmptyMessage);
			}
		}
	}

	public bool HasRows => Rows.Count > 0;

	public bool HasBaseline => (object)Baseline != null;

	public string BaselineLabel
	{
		get
		{
			if ((object)Baseline != null)
			{
				return AppServices.Localizer.Format("benchmark.baselineLabel", GhostFormat.DateTime(Baseline.TakenAt));
			}
			return string.Empty;
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand CaptureBaselineCommand => captureBaselineCommand ?? (captureBaselineCommand = new AsyncRelayCommand(CaptureBaselineAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand CompareCommand => compareCommand ?? (compareCommand = new AsyncRelayCommand(CompareAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ClearBaselineCommand => clearBaselineCommand ?? (clearBaselineCommand = new RelayCommand(ClearBaseline));

	public BenchmarkViewModel()
	{
		_snapshots = AppServices.Snapshots;
		StatusMessage = string.Empty;
		EmptyMessage = string.Empty;
		Baseline = _snapshots.LoadBaseline();
		RefreshMessages();
	}

	public void OnLanguageChanged()
	{
		Rows.Clear();
		StatusMessage = string.Empty;
		RefreshMessages();
	}

	[RelayCommand]
	private async Task CaptureBaselineAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["benchmark.measuring"];
		try
		{
			SystemSnapshot systemSnapshot = await _snapshots.CaptureAsync().ConfigureAwait(continueOnCapturedContext: true);
			_snapshots.SaveBaseline(systemSnapshot);
			Baseline = systemSnapshot;
			Rows.Clear();
			StatusMessage = AppServices.Localizer["benchmark.captured"];
		}
		finally
		{
			IsBusy = false;
			RefreshMessages();
		}
	}

	[RelayCommand]
	private async Task CompareAsync()
	{
		if ((object)Baseline == null)
		{
			return;
		}
		IsBusy = true;
		StatusMessage = AppServices.Localizer["benchmark.measuring"];
		try
		{
			SystemSnapshot after = await _snapshots.CaptureAsync().ConfigureAwait(continueOnCapturedContext: true);
			Rows.Clear();
			foreach (BenchmarkRow item in BuildRows(Baseline, after))
			{
				Rows.Add(item);
			}
			StatusMessage = ((Rows.Count == 0) ? AppServices.Localizer["benchmark.empty.noComparable"] : string.Empty);
		}
		finally
		{
			IsBusy = false;
			RefreshMessages();
		}
	}

	[RelayCommand]
	private void ClearBaseline()
	{
		_snapshots.ClearBaseline();
		Baseline = null;
		Rows.Clear();
		StatusMessage = string.Empty;
		RefreshMessages();
	}

	private static IEnumerable<BenchmarkRow> BuildRows(SystemSnapshot before, SystemSnapshot after)
	{
		ILocalizer loc = AppServices.Localizer;
		double? bootSeconds = before.BootSeconds;
		if (bootSeconds.HasValue)
		{
			double valueOrDefault = bootSeconds.GetValueOrDefault();
			bootSeconds = after.BootSeconds;
			if (bootSeconds.HasValue)
			{
				double valueOrDefault2 = bootSeconds.GetValueOrDefault();
				yield return Row(loc["benchmark.metric.boot"], GhostFormat.Seconds(valueOrDefault), GhostFormat.Seconds(valueOrDefault2), valueOrDefault, valueOrDefault2);
			}
		}
		long? usedMemoryBytes = before.UsedMemoryBytes;
		if (usedMemoryBytes.HasValue)
		{
			long valueOrDefault3 = usedMemoryBytes.GetValueOrDefault();
			usedMemoryBytes = after.UsedMemoryBytes;
			if (usedMemoryBytes.HasValue)
			{
				long valueOrDefault4 = usedMemoryBytes.GetValueOrDefault();
				yield return Row(loc["benchmark.metric.ram"], GhostFormat.Bytes(valueOrDefault3), GhostFormat.Bytes(valueOrDefault4), valueOrDefault3, valueOrDefault4);
			}
		}
		int? startupAppCount = before.StartupAppCount;
		if (startupAppCount.HasValue)
		{
			int valueOrDefault5 = startupAppCount.GetValueOrDefault();
			startupAppCount = after.StartupAppCount;
			if (startupAppCount.HasValue)
			{
				int valueOrDefault6 = startupAppCount.GetValueOrDefault();
				yield return Row(loc["benchmark.metric.startup"], GhostFormat.Integer(valueOrDefault5), GhostFormat.Integer(valueOrDefault6), valueOrDefault5, valueOrDefault6);
			}
		}
		bootSeconds = before.BackgroundCpuPercent;
		if (bootSeconds.HasValue)
		{
			double valueOrDefault7 = bootSeconds.GetValueOrDefault();
			bootSeconds = after.BackgroundCpuPercent;
			if (bootSeconds.HasValue)
			{
				double valueOrDefault8 = bootSeconds.GetValueOrDefault();
				yield return Row(loc["benchmark.metric.cpu"], GhostFormat.Percent(valueOrDefault7), GhostFormat.Percent(valueOrDefault8), valueOrDefault7, valueOrDefault8);
			}
		}
	}

	private static BenchmarkRow Row(string metric, string before, string after, double beforeValue, double afterValue)
	{
		if (beforeValue <= 0.0)
		{
			return new BenchmarkRow(metric, before, after, "—", IsImprovement: false);
		}
		double num = (afterValue - beforeValue) / beforeValue * 100.0;
		string delta;
		if (Math.Abs(num) < 1.0)
		{
			delta = AppServices.Localizer["benchmark.unchanged"];
		}
		else
		{
			delta = ((num < 0.0) ? ("↓ " + GhostFormat.Percent(Math.Abs(num))) : ("↑ " + GhostFormat.Percent(num)));
		}
		return new BenchmarkRow(metric, before, after, delta, num < -1.0);
	}

	private void RefreshMessages()
	{
		BenchmarkViewModel benchmarkViewModel = this;
		string emptyMessage;
		if ((object)Baseline == null)
		{
			emptyMessage = AppServices.Localizer["benchmark.empty.noBaseline"];
		}
		else
		{
			emptyMessage = ((Rows.Count == 0) ? AppServices.Localizer["benchmark.empty.baselineTaken"] : string.Empty);
		}
		benchmarkViewModel.EmptyMessage = emptyMessage;
		OnPropertyChanged("HasRows");
		OnPropertyChanged("HasBaseline");
		OnPropertyChanged("BaselineLabel");
	}
}
