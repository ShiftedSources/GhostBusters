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

public class CategoryPageViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly IOptimizationEngine _engine;

	private readonly string _applyTitleKey;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshStateCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? applyCommand;

	public TweakCategoryViewModel Category { get; }

	public TweakCategory CategoryKind { get; }

	public string ApplyTitle => AppServices.Localizer[_applyTitleKey];

	public int SelectedCount => Category.SelectedCount;

	public int PendingCount => Category.PendingCount;

	public string SelectionSummary
	{
		get
		{
			if (PendingCount != 0)
			{
				return AppServices.Localizer.Format("optimize.selection.someShort", PendingCount);
			}
			return string.Empty;
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

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshStateCommand => refreshStateCommand ?? (refreshStateCommand = new AsyncRelayCommand(RefreshStateAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ApplyCommand => applyCommand ?? (applyCommand = new AsyncRelayCommand(ApplyAsync));

	public CategoryPageViewModel(TweakCategory category, string titleKey, string iconKey, string applyTitleKey)
	{
		_engine = AppServices.Engine;
		_applyTitleKey = applyTitleKey;
		Category = new TweakCategoryViewModel(category, titleKey, iconKey);
		CategoryKind = category;
		StatusMessage = string.Empty;
		ResultMessage = string.Empty;
		Category.SelectionChanged += (object? _, EventArgs _) =>
		{
			OnPropertyChanged("SelectedCount");
			OnPropertyChanged("PendingCount");
			OnPropertyChanged("SelectionSummary");
		};
	}

	public virtual Task ActivatedAsync()
	{
		return RefreshStateAsync();
	}

	public virtual void OnLanguageChanged()
	{
		Category.OnLanguageChanged();
		OnPropertyChanged("SelectionSummary");
		ResultMessage = string.Empty;
		StatusMessage = string.Empty;
	}

	[RelayCommand]
	public async Task RefreshStateAsync()
	{
		IsBusy = true;
		try
		{
			List<string> ids = Category.Items.Select((TweakItemViewModel i) => i.Id).ToList();
			IReadOnlyDictionary<string, TweakApplicability> applicability = await _engine.GetApplicabilityAsync(ids).ConfigureAwait(continueOnCapturedContext: true);
			IReadOnlyDictionary<string, string?> values = await _engine.ReadCurrentValuesAsync(ids).ConfigureAwait(continueOnCapturedContext: true);
			IReadOnlyDictionary<string, bool> dictionary = await _engine.GetAppliedStatesAsync(ids).ConfigureAwait(continueOnCapturedContext: true);
			foreach (TweakItemViewModel item in Category.Items)
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
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	public async Task ApplyAsync()
	{
		if (await AdminGate.RequireAdminAsync().ConfigureAwait(continueOnCapturedContext: true))
		{
			int num = Category.IdsToApply.Count() + Category.IdsToRevert.Count();
			if (num == 0)
			{
				ResultMessage = AppServices.Localizer["result.selection.none"];
			}
			else if (await AppServices.Dialogs.ConfirmAsync(AppServices.Localizer["dialog.apply.title"], AppServices.Localizer.Format("dialog.apply.body", num), AppServices.Localizer["common.apply"]) && ((await ApplyCoreAsync())?.RestartRequired ?? false))
			{
				await AppServices.Dialogs.AlertAsync(AppServices.Localizer["dialog.restart.title"], AppServices.Localizer["dialog.restart.body"]);
			}
		}
	}

	private async Task<ApplyOutcome?> ApplyCoreAsync()
	{
		List<string> list = Category.IdsToApply.ToList();
		List<string> list2 = Category.IdsToRevert.ToList();
		if (list.Count == 0 && list2.Count == 0)
		{
			ResultMessage = AppServices.Localizer["result.selection.none"];
			return null;
		}
		IsBusy = true;
		ResultMessage = string.Empty;
		ApplyOutcome outcome;
		try
		{
			Progress<string> progress = new Progress<string>((string step) =>
			{
				StatusMessage = step;
			});
			outcome = await OptimizeViewModel.ApplyAndRevertAsync(_engine, list, list2, (IReadOnlyList<string> ids) => new ApplyRequest
			{
				TweakIds = ids,
				Title = ApplyTitle,
				Category = CategoryKind,
				CreateRestorePoint = AppServices.Settings.Current.AutoRestorePoint
			}, progress).ConfigureAwait(continueOnCapturedContext: true);
		}
		finally
		{
			IsBusy = false;
			StatusMessage = string.Empty;
		}
		await RefreshStateAsync().ConfigureAwait(continueOnCapturedContext: true);
		ResultMessage = (((object)outcome == null) ? string.Empty : OptimizeViewModel.BuildResultMessage(outcome));
		return outcome;
	}
}
