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

public sealed class OptimizeViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly IOptimizationEngine _engine;

	private HashSet<string>? _pendingSelection;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshStateCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? applyCommand;

	public ObservableCollection<PresetViewModel> Presets { get; } = new ObservableCollection<PresetViewModel>();

	public IReadOnlyList<TweakCategoryViewModel> Categories { get; }

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public PresetViewModel? SelectedPreset
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<PresetViewModel>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectedPreset);
				field = value;
				OnSelectedPresetChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectedPreset);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public int SelectedCount
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SelectedCount);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SelectedCount);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public int PendingCount
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PendingCount);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PendingCount);
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

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string ResultMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ResultMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ResultMessage);
			}
		}
	}

	public string SelectionSummary
	{
		get
		{
			if (PendingCount != 0)
			{
				return AppServices.Localizer.Format("optimize.selection.some", PendingCount);
			}
			return string.Empty;
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshStateCommand => refreshStateCommand ?? (refreshStateCommand = new AsyncRelayCommand(RefreshStateAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ApplyCommand => applyCommand ?? (applyCommand = new AsyncRelayCommand(ApplyAsync));

	public OptimizeViewModel()
	{
		_engine = AppServices.Engine;
		StatusMessage = string.Empty;
		ResultMessage = string.Empty;
		foreach (Preset item in PresetLibrary.All)
		{
			Presets.Add(new PresetViewModel(item));
		}
		Categories = new _003C_003Ez__ReadOnlyArray<TweakCategoryViewModel>(new TweakCategoryViewModel[3]
		{
			new TweakCategoryViewModel(TweakCategory.Performance, "optimize.category.performance", "Icon.Optimize"),
			new TweakCategoryViewModel(TweakCategory.Gaming, "optimize.category.gaming", "Icon.Gaming"),
			new TweakCategoryViewModel(TweakCategory.Network, "optimize.category.network", "Icon.Network")
		});
		foreach (TweakCategoryViewModel category in Categories)
		{
			category.SelectionChanged += (object? _, EventArgs _) =>
			{
				RefreshSelectionSummary();
			};
		}
	}

	private void RefreshSelectionSummary()
	{
		SelectedCount = Categories.Sum((TweakCategoryViewModel c) => c.SelectedCount);
		PendingCount = Categories.Sum((TweakCategoryViewModel c) => c.PendingCount);
		OnPropertyChanged("SelectionSummary");
	}

	public Task ActivatedAsync()
	{
		return RefreshStateAsync();
	}

	public void OnLanguageChanged()
	{
		foreach (PresetViewModel preset in Presets)
		{
			preset.OnLanguageChanged();
		}
		foreach (TweakCategoryViewModel category in Categories)
		{
			category.OnLanguageChanged();
		}
		OnPropertyChanged("SelectionSummary");
		ResultMessage = string.Empty;
		StatusMessage = string.Empty;
	}

	[RelayCommand]
	public async Task RefreshStateAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["optimize.readingState"];
		try
		{
			List<TweakItemViewModel> items = Categories.SelectMany((TweakCategoryViewModel c) => c.Items).ToList();
			List<string> ids = items.Select((TweakItemViewModel i) => i.Id).ToList();
			IReadOnlyDictionary<string, TweakApplicability> applicability = await _engine.GetApplicabilityAsync(ids).ConfigureAwait(continueOnCapturedContext: true);
			IReadOnlyDictionary<string, string?> values = await _engine.ReadCurrentValuesAsync(ids).ConfigureAwait(continueOnCapturedContext: true);
			IReadOnlyDictionary<string, bool> dictionary = await _engine.GetAppliedStatesAsync(ids).ConfigureAwait(continueOnCapturedContext: true);
			foreach (TweakItemViewModel item in items)
			{
				if (applicability.TryGetValue(item.Id, out TweakApplicability value))
				{
					item.IsApplicable = value.IsApplicable;
					item.UnavailableReason = value.Reason ?? string.Empty;
				}
				item.CurrentValue = values.GetValueOrDefault(item.Id);
				item.IsAlreadyApplied = dictionary.GetValueOrDefault(item.Id);
				item.IsSelected = item.IsApplicable && item.IsAlreadyApplied;
			}
			SelectedPreset = null;
			StatusMessage = string.Empty;
		}
		finally
		{
			IsBusy = false;
			HashSet<string> pendingSelection = _pendingSelection;
			if (pendingSelection != null)
			{
				_pendingSelection = null;
				foreach (TweakCategoryViewModel category in Categories)
				{
					category.ApplySelection(pendingSelection);
				}
			}
			RefreshSelectionSummary();
		}
	}

	public void SelectOnly(IEnumerable<string> ids)
	{
		HashSet<string> hashSet = ids.ToHashSet(StringComparer.Ordinal);
		SelectedPreset = null;
		if (IsBusy)
		{
			_pendingSelection = hashSet;
			return;
		}
		foreach (TweakCategoryViewModel category in Categories)
		{
			category.ApplySelection(hashSet);
		}
		RefreshSelectionSummary();
	}

	public IReadOnlyList<string> GetSelectedIds()
	{
		return Categories.SelectMany((TweakCategoryViewModel c) => c.SelectedIds).ToList();
	}

	private IReadOnlyList<string> GetIdsToApply()
	{
		return Categories.SelectMany((TweakCategoryViewModel c) => c.IdsToApply).ToList();
	}

	private IReadOnlyList<string> GetIdsToRevert()
	{
		return Categories.SelectMany((TweakCategoryViewModel c) => c.IdsToRevert).ToList();
	}

	public static async Task<ApplyOutcome?> ApplyAndRevertAsync(IOptimizationEngine engine, IReadOnlyList<string> applyIds, IReadOnlyList<string> revertIds, Func<IReadOnlyList<string>, ApplyRequest> buildRequest, IProgress<string> progress)
	{
		if (applyIds.Count == 0 && revertIds.Count == 0)
		{
			return null;
		}
		List<TweakResult> results = new List<TweakResult>();
		ApplyOutcome applied = null;
		if (applyIds.Count > 0)
		{
			applied = await engine.ApplyAsync(buildRequest(applyIds), progress).ConfigureAwait(continueOnCapturedContext: true);
			results.AddRange(applied.Results);
		}
		if (revertIds.Count > 0)
		{
			results.AddRange((await engine.RevertTweaksAsync(revertIds, progress).ConfigureAwait(continueOnCapturedContext: true)).Results);
		}
		return new ApplyOutcome
		{
			Results = results,
			ChangeSet = applied?.ChangeSet,
			RestorePointWarning = applied?.RestorePointWarning
		};
	}

	[RelayCommand]
	public async Task ApplyAsync()
	{
		if (await AdminGate.RequireAdminAsync().ConfigureAwait(continueOnCapturedContext: true))
		{
			int num = GetIdsToApply().Count + GetIdsToRevert().Count;
			if (num == 0)
			{
				ResultMessage = AppServices.Localizer["result.selection.none"];
			}
			else if (await AppServices.Dialogs.ConfirmAsync(AppServices.Localizer["dialog.apply.title"], AppServices.Localizer.Format("dialog.apply.body", num), AppServices.Localizer["common.apply"]) && ((await ApplyCoreAsync())?.RestartRequired ?? false))
			{
				await AppServices.Dialogs.AlertAsync(AppServices.Localizer["dialog.restart.title"], AppServices.Localizer["dialog.restart.bodyGpu"]);
			}
		}
	}

	private async Task<ApplyOutcome?> ApplyCoreAsync()
	{
		IReadOnlyList<string> idsToApply = GetIdsToApply();
		IReadOnlyList<string> idsToRevert = GetIdsToRevert();
		if (idsToApply.Count == 0 && idsToRevert.Count == 0)
		{
			ResultMessage = AppServices.Localizer["result.selection.none"];
			return null;
		}
		string title = ((SelectedPreset == null) ? AppServices.Localizer["history.title.custom"] : AppServices.Localizer.Format("history.title.preset", SelectedPreset.Name));
		IsBusy = true;
		ResultMessage = string.Empty;
		ApplyOutcome outcome;
		try
		{
			Progress<string> progress = new Progress<string>((string step) =>
			{
				StatusMessage = step;
			});
			outcome = await ApplyAndRevertAsync(_engine, idsToApply, idsToRevert, (IReadOnlyList<string> ids) => new ApplyRequest
			{
				TweakIds = ids,
				Title = title,
				Category = TweakCategory.Performance,
				CreateRestorePoint = AppServices.Settings.Current.AutoRestorePoint
			}, progress).ConfigureAwait(continueOnCapturedContext: true);
		}
		finally
		{
			IsBusy = false;
			StatusMessage = string.Empty;
		}
		await RefreshStateAsync().ConfigureAwait(continueOnCapturedContext: true);
		ResultMessage = (((object)outcome == null) ? string.Empty : BuildResultMessage(outcome));
		return outcome;
	}

	public static string BuildResultMessage(ApplyOutcome outcome)
	{
		ILocalizer localizer = AppServices.Localizer;
		List<string> list = new List<string>();
		if (outcome.SucceededCount > 0)
		{
			list.Add(localizer.Format("result.summary.applied", outcome.SucceededCount));
		}
		if (outcome.FailedCount > 0)
		{
			list.Add(localizer.Format("result.summary.failed", outcome.FailedCount));
		}
		if (outcome.RestartRequired)
		{
			list.Add(localizer["result.summary.restart"]);
		}
		if (outcome.RestorePointWarning != null)
		{
			list.Add(outcome.RestorePointWarning);
		}
		if (list.Count != 0)
		{
			return string.Join(" · ", list);
		}
		return localizer["result.summary.none"];
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnSelectedPresetChanged(PresetViewModel? value)
	{
		if (value == null)
		{
			return;
		}
		foreach (TweakCategoryViewModel category in Categories)
		{
			category.ApplySelection(value.Preset.TweakIds);
		}
		RefreshSelectionSummary();
	}
}
