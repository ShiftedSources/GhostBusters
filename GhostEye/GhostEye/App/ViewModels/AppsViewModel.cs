using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Apps;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class AppsViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly IInstalledAppsService _apps;

	private bool _loaded;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<string?>? sortCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? removeSelectedCommand;

	public ObservableCollection<InstalledAppViewModel> Apps { get; } = new ObservableCollection<InstalledAppViewModel>();

	public ICollectionView AppsView { get; }

	public ObservableCollection<StoreAppViewModel> StoreApps { get; } = new ObservableCollection<StoreAppViewModel>();

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
	public string SearchText
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SearchText);
				field = value;
				OnSearchTextChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SearchText);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool OnlyUnused
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.OnlyUnused);
				field = value;
				OnOnlyUnusedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.OnlyUnused);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string SortBy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SortBy);
				field = value;
				OnSortByChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SortBy);
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

	public bool HasStoreApps => StoreApps.Count > 0;

	public int SelectedCount => Apps.Count((InstalledAppViewModel a) => a.IsSelected) + StoreApps.Count((StoreAppViewModel a) => a.IsSelected);

	public string SummaryLabel => AppServices.Localizer.Format("apps.summary", Apps.Count, GhostFormat.Bytes(Apps.Sum((InstalledAppViewModel a) => a.SizeBytes)), Apps.Count((InstalledAppViewModel a) => a.IsUnused));

	public string SelectionLabel
	{
		get
		{
			if (SelectedCount != 0)
			{
				return AppServices.Localizer.Format("apps.selection.some", SelectedCount);
			}
			return AppServices.Localizer["apps.selection.none"];
		}
	}

	public bool ShowUsageHint => !AdminGate.IsElevated;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<string?> SortCommand => sortCommand ?? (sortCommand = new RelayCommand<string>(Sort));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(RefreshAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RemoveSelectedCommand => removeSelectedCommand ?? (removeSelectedCommand = new AsyncRelayCommand(RemoveSelectedAsync));

	public AppsViewModel()
	{
		_apps = AppServices.InstalledApps;
		SearchText = string.Empty;
		StatusMessage = string.Empty;
		ResultMessage = string.Empty;
		SortBy = "size";
		AppsView = CollectionViewSource.GetDefaultView(Apps);
		AppsView.Filter = Filter;
		ApplySort();
	}

	public async Task ActivatedAsync()
	{
		if (!_loaded)
		{
			_loaded = true;
			await RefreshAsync().ConfigureAwait(continueOnCapturedContext: true);
		}
	}

	public void OnLanguageChanged()
	{
		foreach (InstalledAppViewModel app in Apps)
		{
			app.OnLanguageChanged();
		}
		foreach (StoreAppViewModel storeApp in StoreApps)
		{
			storeApp.OnLanguageChanged();
		}
		RefreshTotals();
	}

	[RelayCommand]
	private void Sort(string? key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			SortBy = key;
		}
	}

	[RelayCommand]
	private async Task RefreshAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["apps.loading"];
		try
		{
			Task<IReadOnlyList<InstalledApp>> desktop = _apps.GetDesktopAppsAsync();
			Task<IReadOnlyList<StoreApp>> store = _apps.GetBloatwareAsync();
			await Task.WhenAll(desktop, store).ConfigureAwait(continueOnCapturedContext: true);
			List<InstalledAppViewModel> list = await Task.Run(() => desktop.Result.Select((InstalledApp a) => new InstalledAppViewModel(a)).ToList()).ConfigureAwait(continueOnCapturedContext: true);
			Apps.Clear();
			foreach (InstalledAppViewModel item in list)
			{
				item.PropertyChanged += OnItemSelectionChanged;
				Apps.Add(item);
			}
			StoreApps.Clear();
			foreach (StoreApp item2 in store.Result)
			{
				StoreAppViewModel storeAppViewModel = new StoreAppViewModel(item2);
				storeAppViewModel.PropertyChanged += OnItemSelectionChanged;
				StoreApps.Add(storeAppViewModel);
			}
			StatusMessage = string.Empty;
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Apps list failed", ex);
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
			RefreshTotals();
		}
	}

	[RelayCommand]
	private async Task RemoveSelectedAsync()
	{
		ILocalizer loc = AppServices.Localizer;
		List<InstalledAppViewModel> desktop = Apps.Where((InstalledAppViewModel a) => a.IsSelected).ToList();
		List<StoreAppViewModel> store = StoreApps.Where((StoreAppViewModel a) => a.IsSelected).ToList();
		if (desktop.Count + store.Count == 0)
		{
			ResultMessage = loc["apps.selection.none"];
		}
		else
		{
			string text = string.Join(", ", desktop.Select((InstalledAppViewModel a) => a.Name).Concat(store.Select((StoreAppViewModel a) => a.Name)).Take(6));
			if (desktop.Count + store.Count > 6)
			{
				text += ", …";
			}
			if (!(await AppServices.Dialogs.ConfirmAsync(loc["apps.confirm.title"], loc.Format("apps.confirm.body", desktop.Count + store.Count, text), loc["common.uninstall"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true)))
			{
				return;
			}
			IsBusy = true;
			ResultMessage = string.Empty;
			int removed = 0;
			int failed = 0;
			try
			{
				foreach (InstalledAppViewModel item in desktop)
				{
					StatusMessage = loc.Format("apps.removing", item.Name);
					item.IsBusy = true;
					OperationResult operationResult = await _apps.UninstallAsync(item.App).ConfigureAwait(continueOnCapturedContext: true);
					item.IsBusy = false;
					if (operationResult.Success)
					{
						removed++;
						AppServices.Logger.Info("Uninstalled: " + item.Name);
					}
					else
					{
						failed++;
						item.Error = operationResult.Error;
					}
				}
				foreach (StoreAppViewModel item2 in store)
				{
					StatusMessage = loc.Format("apps.removing", item2.Name);
					OperationResult operationResult2 = await _apps.RemoveStoreAppAsync(item2.App).ConfigureAwait(continueOnCapturedContext: true);
					if (operationResult2.Success)
					{
						removed++;
						AppServices.Logger.Info("Removed Store app: " + item2.PackageName);
					}
					else
					{
						failed++;
						item2.Error = operationResult2.Error;
					}
				}
			}
			finally
			{
				IsBusy = false;
				StatusMessage = string.Empty;
			}
			if (failed == 0)
			{
				await RefreshAsync().ConfigureAwait(continueOnCapturedContext: true);
			}
			ResultMessage = ((failed == 0) ? loc.Format("apps.result", removed) : loc.Format("apps.resultPartial", removed, failed));
		}
	}

	private void OnItemSelectionChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "IsSelected")
		{
			OnPropertyChanged("SelectedCount");
			OnPropertyChanged("SelectionLabel");
		}
	}

	private void ApplySort()
	{
		if (AppsView == null)
		{
			return;
		}
		using (AppsView.DeferRefresh())
		{
			AppsView.SortDescriptions.Clear();
			string sortBy = SortBy;
			if (!(sortBy == "name"))
			{
				if (sortBy == "lastUsed")
				{
					AppsView.SortDescriptions.Add(new SortDescription("LastUsedSort", ListSortDirection.Ascending));
				}
				else
				{
					AppsView.SortDescriptions.Add(new SortDescription("SizeBytes", ListSortDirection.Descending));
				}
			}
			else
			{
				AppsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
			}
		}
	}

	private bool Filter(object obj)
	{
		if (!(obj is InstalledAppViewModel installedAppViewModel))
		{
			return false;
		}
		if (OnlyUnused && !installedAppViewModel.IsUnused)
		{
			return false;
		}
		if (!string.IsNullOrWhiteSpace(SearchText) && !installedAppViewModel.Name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))
		{
			return installedAppViewModel.Publisher.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
		}
		return true;
	}

	private void RefreshTotals()
	{
		OnPropertyChanged("HasStoreApps");
		OnPropertyChanged("SummaryLabel");
		OnPropertyChanged("SelectedCount");
		OnPropertyChanged("SelectionLabel");
		OnPropertyChanged("ShowUsageHint");
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnSearchTextChanged(string value)
	{
		AppsView?.Refresh();
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnOnlyUnusedChanged(bool value)
	{
		AppsView?.Refresh();
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnSortByChanged(string value)
	{
		ApplySort();
	}
}
