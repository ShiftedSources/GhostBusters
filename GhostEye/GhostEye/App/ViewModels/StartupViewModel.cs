using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class StartupViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly IStartupManagerService _startup;

	private bool _loaded;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<StartupItemViewModel?>? toggleCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<StartupItemViewModel?>? removeCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<StartupItemViewModel?>? restoreCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<StartupItemViewModel?>? openLocationCommand;

	public ObservableCollection<StartupItemViewModel> Items { get; } = new ObservableCollection<StartupItemViewModel>();

	public ICollectionView ItemsView { get; }

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
	public bool ShowMicrosoft
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowMicrosoft);
				field = value;
				OnShowMicrosoftChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowMicrosoft);
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

	public int EnabledCount => Items.Count((StartupItemViewModel i) => i.IsOn && !i.IsRemoved);

	public int DisabledCount => Items.Count((StartupItemViewModel i) => !i.IsOn || i.IsRemoved);

	public string SummaryLabel => AppServices.Localizer.Format("startup.summary", EnabledCount, DisabledCount);

	public bool ShowAdminHint => !AdminGate.IsElevated;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(RefreshAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<StartupItemViewModel?> ToggleCommand => toggleCommand ?? (toggleCommand = new AsyncRelayCommand<StartupItemViewModel>(ToggleAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<StartupItemViewModel?> RemoveCommand => removeCommand ?? (removeCommand = new AsyncRelayCommand<StartupItemViewModel>(RemoveAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<StartupItemViewModel?> RestoreCommand => restoreCommand ?? (restoreCommand = new AsyncRelayCommand<StartupItemViewModel>(RestoreAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<StartupItemViewModel?> OpenLocationCommand => openLocationCommand ?? (openLocationCommand = new RelayCommand<StartupItemViewModel>(OpenLocation));

	public StartupViewModel()
	{
		_startup = AppServices.StartupManager;
		SearchText = string.Empty;
		StatusMessage = string.Empty;
		ItemsView = CollectionViewSource.GetDefaultView(Items);
		ItemsView.Filter = Filter;
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
		foreach (StartupItemViewModel item in Items)
		{
			item.OnLanguageChanged();
		}
		OnPropertyChanged("SummaryLabel");
	}

	[RelayCommand]
	private async Task RefreshAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["startup.loading"];
		try
		{
			IReadOnlyList<StartupEntry> entries = await _startup.GetAllEntriesAsync().ConfigureAwait(continueOnCapturedContext: true);
			List<StartupItemViewModel> list = await Task.Run(() => (from e in entries
				select new StartupItemViewModel(e) into i
				orderby i.IsRemoved, i.IsOn descending
				select i).ThenBy((StartupItemViewModel i) => i.Name, StringComparer.CurrentCultureIgnoreCase).ToList()).ConfigureAwait(continueOnCapturedContext: true);
			Items.Clear();
			foreach (StartupItemViewModel item in list)
			{
				Items.Add(item);
			}
			StatusMessage = ((list.Count == 0) ? AppServices.Localizer["startup.empty"] : string.Empty);
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Startup list failed", ex);
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
			RefreshTotals();
		}
	}

	[RelayCommand]
	private async Task ToggleAsync(StartupItemViewModel? item)
	{
		if (item == null || item.IsBusy)
		{
			return;
		}
		bool wanted = !item.Entry.IsEnabled;
		item.ResetSwitch();
		bool flag = item.Entry.IsMachineWide;
		if (flag)
		{
			flag = !(await AdminGate.RequireAdminAsync("startup").ConfigureAwait(continueOnCapturedContext: true));
		}
		if (!flag)
		{
			await RunOnItemAsync(item, () => _startup.SetEnabledAsync(item.Entry, wanted)).ConfigureAwait(continueOnCapturedContext: true);
		}
	}

	[RelayCommand]
	private async Task RemoveAsync(StartupItemViewModel? item)
	{
		if (item == null || !item.CanRemove)
		{
			return;
		}
		bool flag = item.Entry.IsMachineWide;
		if (flag)
		{
			flag = !(await AdminGate.RequireAdminAsync("startup").ConfigureAwait(continueOnCapturedContext: true));
		}
		if (flag)
		{
			return;
		}
		ILocalizer localizer = AppServices.Localizer;
		if (await AppServices.Dialogs.ConfirmAsync(localizer["startup.remove.title"], localizer.Format("startup.remove.body", item.Name), localizer["common.remove"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true))
		{
			await RunOnItemAsync(item, () => Task.FromResult(_startup.Remove(item.Entry))).ConfigureAwait(continueOnCapturedContext: true);
		}
	}

	[RelayCommand]
	private async Task RestoreAsync(StartupItemViewModel? item)
	{
		if (item == null || !item.IsRemoved)
		{
			return;
		}
		bool flag = item.Entry.IsMachineWide;
		if (flag)
		{
			flag = !(await AdminGate.RequireAdminAsync("startup").ConfigureAwait(continueOnCapturedContext: true));
		}
		if (!flag)
		{
			await RunOnItemAsync(item, () => Task.FromResult(_startup.Restore(item.Entry))).ConfigureAwait(continueOnCapturedContext: true);
		}
	}

	[RelayCommand]
	private void OpenLocation(StartupItemViewModel? item)
	{
		string text = item?.Entry.ExecutablePath;
		if (text != null && File.Exists(text))
		{
			Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + text + "\"")
			{
				UseShellExecute = true
			});
		}
	}

	private async Task RunOnItemAsync(StartupItemViewModel item, Func<Task<StartupOperationResult>> operation)
	{
		item.IsBusy = true;
		item.Error = string.Empty;
		StartupOperationResult startupOperationResult;
		try
		{
			startupOperationResult = await operation().ConfigureAwait(continueOnCapturedContext: true);
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Startup operation failed for " + item.Entry.Id, ex);
			startupOperationResult = StartupOperationResult.Fail(ex.Message);
		}
		finally
		{
			item.IsBusy = false;
		}
		if (!startupOperationResult.Success)
		{
			item.Error = (string.IsNullOrWhiteSpace(startupOperationResult.Error) ? AppServices.Localizer["startup.error.notApplied"] : startupOperationResult.Error);
			return;
		}
		AppServices.Logger.Info("Startup entry changed: " + item.Entry.Id);
		await RefreshAsync().ConfigureAwait(continueOnCapturedContext: true);
	}

	private bool Filter(object obj)
	{
		if (!(obj is StartupItemViewModel startupItemViewModel))
		{
			return false;
		}
		if (!ShowMicrosoft && startupItemViewModel.IsMicrosoft)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(SearchText))
		{
			return true;
		}
		if (!startupItemViewModel.Name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) && !startupItemViewModel.Command.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase))
		{
			return startupItemViewModel.Publisher.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase);
		}
		return true;
	}

	private void RefreshTotals()
	{
		OnPropertyChanged("EnabledCount");
		OnPropertyChanged("DisabledCount");
		OnPropertyChanged("SummaryLabel");
		OnPropertyChanged("ShowAdminHint");
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnSearchTextChanged(string value)
	{
		ItemsView?.Refresh();
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnShowMicrosoftChanged(bool value)
	{
		ItemsView?.Refresh();
	}
}
