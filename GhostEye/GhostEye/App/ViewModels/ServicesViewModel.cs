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
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class ServicesViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly ISystemOptimizerService _optimizer;

	private readonly IServiceBackupService _backups;

	private bool _loaded;

	private bool _autoBackupTaken;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? createBackupCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<ServiceBackupItemViewModel?>? restoreBackupCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<ServiceBackupItemViewModel?>? deleteBackupCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<SystemItemViewModel?>? toggleServiceCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<SystemItemViewModel?>? toggleTaskCommand;

	public ObservableCollection<ServiceBackupItemViewModel> Backups { get; } = new ObservableCollection<ServiceBackupItemViewModel>();

	public bool HasNoBackups => Backups.Count == 0;

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string BackupMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.BackupMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.BackupMessage);
			}
		}
	}

	public ObservableCollection<SystemItemViewModel> Services { get; } = new ObservableCollection<SystemItemViewModel>();

	public ObservableCollection<SystemItemViewModel> Tasks { get; } = new ObservableCollection<SystemItemViewModel>();

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

	public bool ShowAdminHint => !AdminGate.IsElevated;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(RefreshAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand CreateBackupCommand => createBackupCommand ?? (createBackupCommand = new AsyncRelayCommand(CreateBackupAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<ServiceBackupItemViewModel?> RestoreBackupCommand => restoreBackupCommand ?? (restoreBackupCommand = new AsyncRelayCommand<ServiceBackupItemViewModel>(RestoreBackupAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<ServiceBackupItemViewModel?> DeleteBackupCommand => deleteBackupCommand ?? (deleteBackupCommand = new AsyncRelayCommand<ServiceBackupItemViewModel>(DeleteBackupAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<SystemItemViewModel?> ToggleServiceCommand => toggleServiceCommand ?? (toggleServiceCommand = new AsyncRelayCommand<SystemItemViewModel>(ToggleServiceAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<SystemItemViewModel?> ToggleTaskCommand => toggleTaskCommand ?? (toggleTaskCommand = new AsyncRelayCommand<SystemItemViewModel>(ToggleTaskAsync));

	public ServicesViewModel()
	{
		_optimizer = AppServices.SystemOptimizer;
		_backups = AppServices.ServiceBackups;
		StatusMessage = string.Empty;
		BackupMessage = string.Empty;
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
		foreach (SystemItemViewModel item in Services.Concat(Tasks))
		{
			item.OnLanguageChanged();
		}
		BackupMessage = string.Empty;
		RefreshAsync();
	}

	[RelayCommand]
	private async Task RefreshAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["services.loading"];
		ILocalizer loc = AppServices.Localizer;
		try
		{
			IReadOnlyList<OptionalServiceState> services = await Task.Run((Func<IReadOnlyList<OptionalServiceState>>)_optimizer.GetServices).ConfigureAwait(continueOnCapturedContext: true);
			IReadOnlyList<OptionalTaskState> readOnlyList = await _optimizer.GetTasksAsync().ConfigureAwait(continueOnCapturedContext: true);
			Services.Clear();
			foreach (OptionalServiceState item in services)
			{
				Services.Add(new SystemItemViewModel(item.Definition.Name, item.Definition.NameKey, item.Definition.DescriptionKey, item.Definition.Risk, item.Definition.Name)
				{
					ToggleCommand = ToggleServiceCommand,
					IsAvailable = item.IsInstalled,
					IsOptimized = item.IsOptimized,
					StateLabel = ((!item.IsInstalled) ? loc["services.state.notInstalled"] : loc.Format("services.state.service", loc["services.status." + (item.Status?.ToString() ?? "Unknown")], loc["services.mode." + (item.StartupMode?.ToString() ?? "Unknown")]))
				});
			}
			Tasks.Clear();
			foreach (OptionalTaskState item2 in readOnlyList)
			{
				Tasks.Add(new SystemItemViewModel(item2.Definition.Path, item2.Definition.NameKey, item2.Definition.DescriptionKey, item2.Definition.Risk, item2.Definition.Path)
				{
					ToggleCommand = ToggleTaskCommand,
					IsAvailable = item2.Exists,
					IsOptimized = item2.IsOptimized,
					StateLabel = ((!item2.Exists) ? loc["services.state.notInstalled"] : (item2.IsEnabled ? loc["services.state.taskEnabled"] : loc["services.state.taskDisabled"]))
				});
			}
			StatusMessage = string.Empty;
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Services list failed", ex);
			StatusMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
			OnPropertyChanged("ShowAdminHint");
			LoadBackups();
		}
	}

	private void LoadBackups()
	{
		Backups.Clear();
		foreach (ServiceBackup item in _backups.List())
		{
			Backups.Add(new ServiceBackupItemViewModel(item));
		}
		OnPropertyChanged("HasNoBackups");
	}

	[RelayCommand]
	private async Task CreateBackupAsync()
	{
		bool flag = IsBusy;
		if (flag)
		{
			return;
		}
		IsBusy = true;
		try
		{
			await _backups.CreateAsync(automatic: false).ConfigureAwait(continueOnCapturedContext: true);
			BackupMessage = AppServices.Localizer["services.backup.created"];
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Service backup failed", ex);
			BackupMessage = AppServices.Localizer.Format("services.backup.createFailed", ex.Message);
		}
		finally
		{
			IsBusy = false;
			LoadBackups();
		}
	}

	[RelayCommand]
	private async Task RestoreBackupAsync(ServiceBackupItemViewModel? item)
	{
		if (item == null || IsBusy)
		{
			return;
		}
		bool flag = !(await AdminGate.RequireAdminAsync("services").ConfigureAwait(continueOnCapturedContext: true));
		if (flag)
		{
			return;
		}
		ILocalizer loc = AppServices.Localizer;
		if (!(await AppServices.Dialogs.ConfirmAsync(loc["services.backup.restore.title"], loc.Format("services.backup.restore.body", item.DateLabel), loc["common.restore"]).ConfigureAwait(continueOnCapturedContext: true)))
		{
			return;
		}
		IsBusy = true;
		BackupMessage = string.Empty;
		try
		{
			await _backups.CreateAsync(automatic: true).ConfigureAwait(continueOnCapturedContext: true);
			_autoBackupTaken = true;
			ServiceRestoreResult serviceRestoreResult = await _backups.RestoreAsync(item.Backup).ConfigureAwait(continueOnCapturedContext: true);
			BackupMessage = ((serviceRestoreResult.Failed.Count == 0) ? loc.Format("services.backup.restored", serviceRestoreResult.Restored) : loc.Format("services.backup.restoredPartial", serviceRestoreResult.Restored, string.Join(", ", serviceRestoreResult.Failed)));
			AppServices.Logger.Info($"Service backup {item.Backup.Id} restored: {serviceRestoreResult.Restored} ok, {serviceRestoreResult.Failed.Count} failed");
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Service backup restore failed for " + item.Backup.Id, ex);
			BackupMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
		}
		await RefreshAsync().ConfigureAwait(continueOnCapturedContext: true);
	}

	[RelayCommand]
	private async Task DeleteBackupAsync(ServiceBackupItemViewModel? item)
	{
		if (item != null && !IsBusy)
		{
			ILocalizer localizer = AppServices.Localizer;
			if (await AppServices.Dialogs.ConfirmAsync(localizer["services.backup.delete.title"], localizer.Format("services.backup.delete.body", item.DateLabel), localizer["common.delete"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true))
			{
				_backups.Delete(item.Backup);
				BackupMessage = string.Empty;
				LoadBackups();
			}
		}
	}

	[RelayCommand]
	private async Task ToggleServiceAsync(SystemItemViewModel? item)
	{
		if (item == null)
		{
			return;
		}
		ServiceTweakDefinition definition = ServiceCatalog.Services.First((ServiceTweakDefinition s) => s.Name == item.Id);
		await ToggleAsync(item, (bool wanted) => Task.Run(() => _optimizer.SetServiceOptimized(definition, wanted))).ConfigureAwait(continueOnCapturedContext: true);
	}

	[RelayCommand]
	private async Task ToggleTaskAsync(SystemItemViewModel? item)
	{
		if (item != null)
		{
			TaskTweakDefinition definition = ServiceCatalog.Tasks.First((TaskTweakDefinition t) => t.Path == item.Id);
			await ToggleAsync(item, (bool wanted) => _optimizer.SetTaskOptimizedAsync(definition, wanted)).ConfigureAwait(continueOnCapturedContext: true);
		}
	}

	private async Task ToggleAsync(SystemItemViewModel item, Func<bool, Task<OperationResult>> operation)
	{
		if (item.IsBusy)
		{
			return;
		}
		bool wanted = !item.IsOptimized;
		item.ResetSwitch();
		if (!(await AdminGate.RequireAdminAsync("services").ConfigureAwait(continueOnCapturedContext: true)))
		{
			return;
		}
		if (wanted && item.Risk == RiskLevel.High)
		{
			ILocalizer localizer = AppServices.Localizer;
			if (!(await AppServices.Dialogs.ConfirmAsync(localizer["services.highRisk.title"], localizer.Format("services.highRisk.body", item.Name, item.Description), localizer["common.disable"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true)))
			{
				return;
			}
		}
		item.IsBusy = true;
		item.Error = string.Empty;
		if (!_autoBackupTaken)
		{
			try
			{
				await _backups.CreateAsync(automatic: true).ConfigureAwait(continueOnCapturedContext: true);
				_autoBackupTaken = true;
				LoadBackups();
			}
			catch (Exception exception)
			{
				AppServices.Logger.Error("Automatic service backup failed", exception);
			}
		}
		OperationResult operationResult;
		try
		{
			operationResult = await operation(wanted).ConfigureAwait(continueOnCapturedContext: true);
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Service/task change failed for " + item.Id, ex);
			operationResult = OperationResult.Fail(ex.Message);
		}
		finally
		{
			item.IsBusy = false;
		}
		if (!operationResult.Success)
		{
			AppServices.Logger.Warn($"{item.Id} change to optimized = {wanted} failed (elevated = {AdminGate.IsElevated}): {operationResult.Error}");
			item.Error = operationResult.Error;
		}
		else
		{
			AppServices.Logger.Info($"{item.Id} optimized = {wanted}");
			await RefreshAsync().ConfigureAwait(continueOnCapturedContext: true);
		}
	}
}
