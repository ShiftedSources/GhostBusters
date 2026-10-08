using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class AnalyzerViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly ISystemAnalyzer _analyzer;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? runCommand;

	public IReadOnlyList<AnalysisGroupViewModel> Groups { get; }

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
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Score);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasRun
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasRun);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasRun);
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

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RunCommand => runCommand ?? (runCommand = new AsyncRelayCommand(RunAsync));

	public AnalyzerViewModel()
	{
		_analyzer = AppServices.Analyzer;
		StatusMessage = string.Empty;
		EmptyMessage = AppServices.Localizer["analyzer.empty.notRun"];
		Groups = new _003C_003Ez__ReadOnlyArray<AnalysisGroupViewModel>(new AnalysisGroupViewModel[3]
		{
			new AnalysisGroupViewModel(AnalysisGroup.Performance, "analyzer.group.performance"),
			new AnalysisGroupViewModel(AnalysisGroup.Windows, "analyzer.group.windows"),
			new AnalysisGroupViewModel(AnalysisGroup.Network, "analyzer.group.network")
		});
	}

	public Task ActivatedAsync()
	{
		return Task.CompletedTask;
	}

	public void OnLanguageChanged()
	{
		foreach (AnalysisGroupViewModel group in Groups)
		{
			group.OnLanguageChanged();
		}
		if (HasRun)
		{
			foreach (AnalysisGroupViewModel group2 in Groups)
			{
				group2.Replace(Array.Empty<AnalysisItem>());
			}
			HasRun = false;
			Score = 0;
		}
		StatusMessage = string.Empty;
		EmptyMessage = AppServices.Localizer["analyzer.empty.notRun"];
	}

	[RelayCommand]
	public async Task RunAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["analyzer.running"];
		try
		{
			Progress<string> progress = new Progress<string>((string step) =>
			{
				StatusMessage = AppServices.Localizer.Format("analyzer.runningStep", step);
			});
			AnalysisReport analysisReport = await _analyzer.AnalyzeAsync(progress).ConfigureAwait(continueOnCapturedContext: true);
			foreach (AnalysisGroupViewModel group in Groups)
			{
				group.Replace(analysisReport.Items.Where((AnalysisItem i) => i.Group == group.Group));
			}
			Score = analysisReport.Score;
			HasRun = true;
			EmptyMessage = ((analysisReport.Items.Count == 0) ? AppServices.Localizer["analyzer.empty.noProbes"] : string.Empty);
			StatusMessage = ((analysisReport.Items.Count == 0) ? string.Empty : AppServices.Localizer.Format("analyzer.completed", analysisReport.Warnings.Count()));
		}
		finally
		{
			IsBusy = false;
		}
	}
}
