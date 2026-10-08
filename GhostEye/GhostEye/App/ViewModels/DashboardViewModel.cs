using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;
using GhostEye.Infrastructure.Probes;

namespace GhostEye.App.ViewModels;

public sealed class DashboardViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel, IDisposable
{
	private readonly ISystemAnalyzer _analyzer;

	private readonly IChangeHistoryStore _history;

	private readonly CpuProbe _cpu;

	private readonly DispatcherTimer _timer;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? reapplyDriftCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<ChangeSetViewModel?>? deleteChangeCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? openHistoryCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? dismissDriftCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? loadCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? analyzeCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? fixAllCommand;

	public ObservableCollection<string> DriftedNames { get; } = new ObservableCollection<string>();

	public bool HasDrift => DriftedNames.Count > 0;

	public string DriftTitle => AppServices.Localizer.Format("drift.card.title", DriftedNames.Count);

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string DriftMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DriftMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DriftMessage);
			}
		}
	} = string.Empty;

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsDriftBusy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsDriftBusy);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsDriftBusy);
			}
		}
	}

	public ObservableCollection<AnalysisItem> Findings { get; } = new ObservableCollection<AnalysisItem>();

	public ObservableCollection<ChangeSetViewModel> RecentChanges { get; } = new ObservableCollection<ChangeSetViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public int Score
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Score);
				field = value;
				OnScoreChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Score);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasScore
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasScore);
				field = value;
				OnHasScoreChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasScore);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string ScoreHeadline
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ScoreHeadline);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ScoreHeadline);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string ScoreSubtitle
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ScoreSubtitle);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ScoreSubtitle);
			}
		}
	}

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
	public string CpuValue
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CpuValue);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CpuValue);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string RamValue
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RamValue);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RamValue);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string GpuValue
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.GpuValue);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.GpuValue);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string GpuTooltip
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.GpuTooltip);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.GpuTooltip);
			}
		}
	} = string.Empty;

	public bool HasFindings => Findings.Count > 0;

	public bool HasRecentChanges => RecentChanges.Count > 0;

	public string ScoreLabel
	{
		get
		{
			if (!HasScore)
			{
				return "—";
			}
			return Score.ToString(AppServices.Localizer.Culture);
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ReapplyDriftCommand => reapplyDriftCommand ?? (reapplyDriftCommand = new AsyncRelayCommand(ReapplyDriftAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<ChangeSetViewModel?> DeleteChangeCommand => deleteChangeCommand ?? (deleteChangeCommand = new AsyncRelayCommand<ChangeSetViewModel>(DeleteChangeAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand OpenHistoryCommand => openHistoryCommand ?? (openHistoryCommand = new RelayCommand(OpenHistory));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand DismissDriftCommand => dismissDriftCommand ?? (dismissDriftCommand = new RelayCommand(DismissDrift));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand LoadCommand => loadCommand ?? (loadCommand = new AsyncRelayCommand(LoadAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AnalyzeCommand => analyzeCommand ?? (analyzeCommand = new AsyncRelayCommand(AnalyzeAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand FixAllCommand => fixAllCommand ?? (fixAllCommand = new RelayCommand(FixAll));

	public DashboardViewModel()
	{
		_analyzer = AppServices.Analyzer;
		_history = AppServices.History;
		_cpu = new CpuProbe(AppServices.Localizer);
		ScoreHeadline = AppServices.Localizer["dashboard.score.notRun"];
		ScoreSubtitle = AppServices.Localizer["dashboard.score.notRunSub"];
		CpuValue = "—";
		RamValue = "—";
		GpuAdapter primary = GpuInfo.GetPrimary(AppServices.Registry);
		GpuValue = primary?.ShortName ?? "—";
		GpuTooltip = primary?.Name ?? string.Empty;
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1.0)
		};
		_timer.Tick += (object? _, EventArgs _) =>
		{
			SampleCounters();
		};
		BackgroundAgent.DriftChanged += OnDriftChanged;
		LoadDrift();
	}

	private void OnDriftChanged(object? sender, EventArgs e)
	{
		LoadDrift();
	}

	private void LoadDrift()
	{
		DriftedNames.Clear();
		foreach (string driftedTweakId in BackgroundAgent.LatestDrift.DriftedTweakIds)
		{
			TweakDefinition tweakDefinition = TweakCatalog.Find(driftedTweakId);
			DriftedNames.Add(((object)tweakDefinition == null) ? driftedTweakId : AppServices.Localizer[tweakDefinition.NameKey]);
		}
		OnPropertyChanged("HasDrift");
		OnPropertyChanged("DriftTitle");
	}

	[RelayCommand]
	private async Task ReapplyDriftAsync()
	{
		List<string> ids = BackgroundAgent.LatestDrift.DriftedTweakIds.ToList();
		bool flag = ids.Count == 0;
		bool flag2 = flag;
		if (!flag2)
		{
			flag2 = !(await AdminGate.RequireAdminAsync("dashboard"));
		}
		if (flag2)
		{
			return;
		}
		IsDriftBusy = true;
		try
		{
			ApplyOutcome applyOutcome = await AppServices.Engine.ReapplyTweaksAsync(ids);
			BackgroundAgent.ClearDrift(from r in applyOutcome.Results
				where r.Success
				select r.TweakId);
			DriftMessage = ((applyOutcome.FailedCount == 0) ? AppServices.Localizer.Format("drift.reapplied", applyOutcome.SucceededCount) : AppServices.Localizer.Format("drift.reappliedPartial", applyOutcome.SucceededCount, applyOutcome.FailedCount));
			if (applyOutcome.RestartRequired)
			{
				DriftMessage = DriftMessage + " " + AppServices.Localizer["drift.restart"];
			}
		}
		finally
		{
			IsDriftBusy = false;
		}
	}

	[RelayCommand]
	private async Task DeleteChangeAsync(ChangeSetViewModel? item)
	{
		bool flag = item != null;
		if (flag)
		{
			flag = await HistoryActions.ConfirmAndDeleteAsync(item.ChangeSet);
		}
		if (flag)
		{
			await LoadAsync();
		}
	}

	[RelayCommand]
	private void OpenHistory()
	{
		AppServices.Shell.NavigateTo("history");
	}

	[RelayCommand]
	private void DismissDrift()
	{
		BackgroundAgent.DismissDrift(BackgroundAgent.LatestDrift.DriftedTweakIds);
		DriftMessage = string.Empty;
	}

	public Task ActivatedAsync()
	{
		return LoadAsync();
	}

	public void OnLanguageChanged()
	{
		Findings.Clear();
		HasScore = false;
		Score = 0;
		ScoreHeadline = AppServices.Localizer["dashboard.score.notRun"];
		ScoreSubtitle = AppServices.Localizer["dashboard.score.notRunSub"];
		foreach (ChangeSetViewModel recentChange in RecentChanges)
		{
			recentChange.OnLanguageChanged();
		}
		OnPropertyChanged("HasFindings");
		DriftMessage = string.Empty;
		LoadDrift();
	}

	public void StartLiveCounters()
	{
		SampleCounters();
		_timer.Start();
	}

	public void StopLiveCounters()
	{
		_timer.Stop();
	}

	private void SampleCounters()
	{
		double? num = _cpu.Sample();
		string cpuValue;
		if (num.HasValue)
		{
			double valueOrDefault = num.GetValueOrDefault();
			cpuValue = Math.Round(valueOrDefault).ToString(AppServices.Localizer.Culture) + "%";
		}
		else
		{
			cpuValue = "—";
		}
		CpuValue = cpuValue;
		(ulong, ulong)? tuple = RamProbe.Read();
		if (tuple.HasValue)
		{
			(ulong, ulong) valueOrDefault2 = tuple.GetValueOrDefault();
			double value = (double)valueOrDefault2.Item1 / 1073741824.0;
			double value2 = (double)valueOrDefault2.Item2 / 1073741824.0;
			RamValue = GhostFormat.Number(value) + " / " + GhostFormat.Number(value2, 0) + " GB";
		}
	}

	[RelayCommand]
	public async Task LoadAsync()
	{
		IsBusy = true;
		try
		{
			IReadOnlyList<ChangeSet> source = await _history.GetAllAsync().ConfigureAwait(continueOnCapturedContext: true);
			RecentChanges.Clear();
			foreach (ChangeSet item in source.Take(3))
			{
				RecentChanges.Add(new ChangeSetViewModel(item));
			}
			OnPropertyChanged("HasRecentChanges");
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	public async Task AnalyzeAsync()
	{
		IsBusy = true;
		try
		{
			AnalysisReport analysisReport = await _analyzer.AnalyzeAsync().ConfigureAwait(continueOnCapturedContext: true);
			ILocalizer localizer = AppServices.Localizer;
			Findings.Clear();
			foreach (AnalysisItem warning in analysisReport.Warnings)
			{
				Findings.Add(warning);
			}
			if (analysisReport.Items.Count == 0)
			{
				HasScore = false;
				ScoreHeadline = localizer["dashboard.score.noProbes"];
				ScoreSubtitle = localizer["dashboard.score.noProbesSub"];
			}
			else
			{
				Score = analysisReport.Score;
				HasScore = true;
				ILocalizer localizer2 = localizer;
				int score = analysisReport.Score;
				string key;
				if (score >= 70)
				{
					key = ((score < 90) ? "dashboard.score.good" : "dashboard.score.excellent");
				}
				else
				{
					key = ((score < 50) ? "dashboard.score.action" : "dashboard.score.improvable");
				}
				ScoreHeadline = localizer2[key];
				int num = analysisReport.Warnings.Count();
				ScoreSubtitle = ((num == 0) ? localizer["dashboard.score.noWarnings"] : localizer.Format("dashboard.score.warnings", num));
			}
			OnPropertyChanged("HasFindings");
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private void FixAll()
	{
		List<string> list = (from f in Findings
			select f.SuggestedTweakId into id
			where !string.IsNullOrWhiteSpace(id)
			select (id)).Distinct(StringComparer.Ordinal).ToList();
		AppServices.Shell.NavigateTo("optimize");
		if (list.Count > 0)
		{
			AppServices.Shell.Find<OptimizeViewModel>()?.SelectOnly(list);
		}
	}

	public void Dispose()
	{
		BackgroundAgent.DriftChanged -= OnDriftChanged;
		_timer.Stop();
		_cpu.Dispose();
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnScoreChanged(int value)
	{
		OnPropertyChanged("ScoreLabel");
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnHasScoreChanged(bool value)
	{
		OnPropertyChanged("ScoreLabel");
	}
}
