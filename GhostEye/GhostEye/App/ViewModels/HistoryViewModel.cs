using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;

namespace GhostEye.App.ViewModels;

public sealed class HistoryViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly IChangeHistoryStore _store;

	private readonly IOptimizationEngine _engine;

	private List<ChangeSet> _all = new List<ChangeSet>();

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<string>? setFilterCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? loadCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<ChangeSetViewModel?>? restoreCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<ChangeSetViewModel?>? deleteCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? clearCommand;

	public ObservableCollection<ChangeSetViewModel> Items { get; } = new ObservableCollection<ChangeSetViewModel>();

	public IReadOnlyList<HistoryFilterViewModel> Filters { get; }

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string SelectedFilter
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectedFilter);
				field = value;
				OnSelectedFilterChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectedFilter);
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

	public bool IsEmpty => Items.Count == 0;

	public bool HasAny => _all.Count > 0;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string> SetFilterCommand => setFilterCommand ?? (setFilterCommand = new RelayCommand<string>(SetFilter));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand LoadCommand => loadCommand ?? (loadCommand = new AsyncRelayCommand(LoadAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<ChangeSetViewModel?> RestoreCommand => restoreCommand ?? (restoreCommand = new AsyncRelayCommand<ChangeSetViewModel>(RestoreAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<ChangeSetViewModel?> DeleteCommand => deleteCommand ?? (deleteCommand = new AsyncRelayCommand<ChangeSetViewModel>(DeleteAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ClearCommand => clearCommand ?? (clearCommand = new AsyncRelayCommand(ClearAsync));

	public HistoryViewModel()
	{
		_store = AppServices.History;
		_engine = AppServices.Engine;
		SelectedFilter = "all";
		StatusMessage = string.Empty;
		Filters = new _003C_003Ez__ReadOnlyArray<HistoryFilterViewModel>(new HistoryFilterViewModel[6]
		{
			new HistoryFilterViewModel("all", "history.filter.all")
			{
				IsSelected = true
			},
			new HistoryFilterViewModel("Performance", "history.filter.performance"),
			new HistoryFilterViewModel("Gaming", "history.filter.gaming"),
			new HistoryFilterViewModel("Network", "history.filter.network"),
			new HistoryFilterViewModel("Privacy", "history.filter.privacy"),
			new HistoryFilterViewModel("Interface", "history.filter.interface")
		});
	}

	public Task ActivatedAsync()
	{
		return LoadAsync();
	}

	public void OnLanguageChanged()
	{
		foreach (HistoryFilterViewModel filter in Filters)
		{
			filter.OnLanguageChanged();
		}
		foreach (ChangeSetViewModel item in Items)
		{
			item.OnLanguageChanged();
		}
		StatusMessage = string.Empty;
	}

	[RelayCommand]
	private void SetFilter(string filter)
	{
		SelectedFilter = filter;
		foreach (HistoryFilterViewModel filter2 in Filters)
		{
			filter2.IsSelected = filter2.Id == filter;
			filter2.RefreshSelection();
		}
	}

	[RelayCommand]
	public async Task LoadAsync()
	{
		IsBusy = true;
		try
		{
			_all = (await _store.GetAllAsync().ConfigureAwait(continueOnCapturedContext: true)).ToList();
			ApplyFilter();
		}
		finally
		{
			IsBusy = false;
		}
	}

	private bool Matches(ChangeSet changeSet)
	{
		if (!(SelectedFilter == "all") && !changeSet.Category.ToString().Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase))
		{
			return changeSet.Changes.Any((TweakChangeRecord c) => TweakCatalog.Find(c.TweakId)?.Category.ToString().Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase) ?? false);
		}
		return true;
	}

	private void ApplyFilter()
	{
		HashSet<string> hashSet = (from i in Items
			where i.IsExpanded
			select i.ChangeSet.Id).ToHashSet();
		Items.Clear();
		foreach (ChangeSet item in _all.Where(Matches))
		{
			Items.Add(new ChangeSetViewModel(item)
			{
				IsExpanded = hashSet.Contains(item.Id)
			});
		}
		OnPropertyChanged("IsEmpty");
		OnPropertyChanged("HasAny");
	}

	[RelayCommand]
	private async Task RestoreAsync(ChangeSetViewModel? item)
	{
		if (item != null && item.CanRestore && !IsBusy && await AdminGate.RequireAdminAsync().ConfigureAwait(continueOnCapturedContext: true))
		{
			ILocalizer localizer = AppServices.Localizer;
			if (await AppServices.Dialogs.ConfirmAsync(localizer["history.confirm.title"], localizer.Format("history.confirm.body", item.ActiveCount, item.Title), localizer["common.restore"], null, destructive: true))
			{
				await RestoreCoreAsync(item);
			}
		}
	}

	private async Task<ApplyOutcome> RestoreCoreAsync(ChangeSetViewModel item)
	{
		ILocalizer loc = AppServices.Localizer;
		IsBusy = true;
		StatusMessage = loc["history.restoring"];
		try
		{
			Progress<string> progress = new Progress<string>((string step) =>
			{
				StatusMessage = loc.Format("history.restoringStep", step);
			});
			ApplyOutcome outcome = await _engine.RevertChangeSetAsync(item.ChangeSet, progress).ConfigureAwait(continueOnCapturedContext: true);
			if ((object)outcome.ChangeSet != null)
			{
				item.Refresh(outcome.ChangeSet);
				int num = _all.FindIndex((ChangeSet c) => c.Id == outcome.ChangeSet.Id);
				if (num >= 0)
				{
					_all[num] = outcome.ChangeSet;
				}
			}
			StatusMessage = ((outcome.FailedCount == 0) ? loc["history.restore.done"] : loc.Format("history.restore.partial", outcome.FailedCount));
			BackgroundAgent.RefreshAfterHistoryChange();
			return outcome;
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task DeleteAsync(ChangeSetViewModel? item)
	{
		if (item != null && !IsBusy && await HistoryActions.ConfirmAndDeleteAsync(item.ChangeSet))
		{
			_all.RemoveAll((ChangeSet c) => c.Id == item.ChangeSet.Id);
			Items.Remove(item);
			StatusMessage = AppServices.Localizer["history.deleted"];
			OnPropertyChanged("IsEmpty");
			OnPropertyChanged("HasAny");
		}
	}

	[RelayCommand]
	private async Task ClearAsync()
	{
		if (!IsBusy && await HistoryActions.ConfirmAndClearAsync(_all))
		{
			_all.Clear();
			Items.Clear();
			StatusMessage = AppServices.Localizer["history.cleared"];
			OnPropertyChanged("IsEmpty");
			OnPropertyChanged("HasAny");
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnSelectedFilterChanged(string value)
	{
		ApplyFilter();
	}
}
