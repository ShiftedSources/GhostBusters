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
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class GameBoostViewModel : ObservableObject
{
	private readonly IGameBoostService _boost;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? refreshCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? startCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? stopCommand;

	public ObservableCollection<BoostAppViewModel> Apps { get; } = new ObservableCollection<BoostAppViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsActive
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsActive);
				field = value;
				OnIsActiveChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsActive);
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
	public bool SwitchPowerPlan
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SwitchPowerPlan);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SwitchPowerPlan);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool PauseUpdates
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PauseUpdates);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PauseUpdates);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool FreeMemory
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.FreeMemory);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.FreeMemory);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ReopenApps
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ReopenApps);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ReopenApps);
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

	public bool HasApps => Apps.Count > 0;

	public bool IsInactive => !IsActive;

	public string ActiveSince
	{
		get
		{
			BoostState state = _boost.State;
			if ((object)state == null)
			{
				return string.Empty;
			}
			return AppServices.Localizer.Format("boost.activeSince", state.StartedAt.LocalDateTime.ToString("t", AppServices.Localizer.Culture));
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new RelayCommand(Refresh));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand StartCommand => startCommand ?? (startCommand = new AsyncRelayCommand(StartAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand StopCommand => stopCommand ?? (stopCommand = new AsyncRelayCommand(StopAsync));

	public GameBoostViewModel()
	{
		_boost = AppServices.GameBoost;
		SwitchPowerPlan = true;
		PauseUpdates = true;
		FreeMemory = true;
		ReopenApps = true;
		ResultMessage = string.Empty;
		Refresh();
	}

	[RelayCommand]
	public void Refresh()
	{
		IsActive = _boost.IsActive;
		Dictionary<string, bool> dictionary = Apps.ToDictionary((BoostAppViewModel a) => a.App.ProcessName, (BoostAppViewModel a) => a.IsSelected, StringComparer.OrdinalIgnoreCase);
		Apps.Clear();
		foreach (BoostApp closableApp in _boost.GetClosableApps())
		{
			Apps.Add(new BoostAppViewModel(closableApp)
			{
				IsSelected = (dictionary.TryGetValue(closableApp.ProcessName, out var value) ? value : closableApp.SelectedByDefault)
			});
		}
		OnPropertyChanged("HasApps");
		OnPropertyChanged("ActiveSince");
	}

	public void OnLanguageChanged()
	{
		ResultMessage = string.Empty;
		OnPropertyChanged("ActiveSince");
	}

	[RelayCommand]
	private async Task StartAsync()
	{
		bool flag = SwitchPowerPlan || PauseUpdates;
		if (flag)
		{
			flag = !(await AdminGate.RequireAdminAsync("gaming"));
		}
		if (flag)
		{
			return;
		}
		IsBusy = true;
		ResultMessage = AppServices.Localizer["boost.starting"];
		try
		{
			BoostOptions options = new BoostOptions(SwitchPowerPlan, PauseUpdates, FreeMemory, (from a in Apps
				where a.IsSelected
				select a.App.ProcessName).ToList());
			BoostReport boostReport = await _boost.StartAsync(options);
			ILocalizer localizer = AppServices.Localizer;
			List<string> list = new List<string> { localizer["boost.started"] };
			string powerPlan = boostReport.PowerPlan;
			if (powerPlan != null)
			{
				list.Add(localizer.Format("boost.plan", powerPlan));
			}
			if (boostReport.ClosedApps > 0)
			{
				list.Add(localizer.Format("boost.closed", boostReport.ClosedApps));
			}
			if (boostReport.FreedBytes > 0)
			{
				list.Add(localizer.Format("boost.freed", GhostFormat.Bytes(boostReport.FreedBytes)));
			}
			list.AddRange(boostReport.Warnings);
			ResultMessage = string.Join(" · ", list);
			AppServices.Logger.Info("Game Boost started.");
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Game Boost start failed", ex);
			ResultMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
			Refresh();
		}
	}

	[RelayCommand]
	private async Task StopAsync()
	{
		BoostState state = _boost.State;
		bool flag;
		if ((object)state != null)
		{
			IReadOnlyList<string> stoppedServices = state.StoppedServices;
			if ((stoppedServices != null && stoppedServices.Count > 0) || state.PreviousPowerPlan.HasValue)
			{
				flag = true;
				goto IL_0056;
			}
		}
		flag = false;
		goto IL_0056;
		IL_0056:
		bool flag2 = flag;
		if (flag2)
		{
			flag2 = !(await AdminGate.RequireAdminAsync("gaming"));
		}
		if (flag2)
		{
			return;
		}
		IsBusy = true;
		try
		{
			BoostReport boostReport = await _boost.StopAsync(ReopenApps);
			ILocalizer localizer = AppServices.Localizer;
			List<string> list = new List<string> { localizer["boost.stopped"] };
			if (boostReport.ClosedApps > 0)
			{
				list.Add(localizer.Format("boost.reopened", boostReport.ClosedApps));
			}
			list.AddRange(boostReport.Warnings);
			ResultMessage = string.Join(" · ", list);
			AppServices.Logger.Info("Game Boost stopped.");
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Game Boost stop failed", ex);
			ResultMessage = ex.Message;
		}
		finally
		{
			IsBusy = false;
			Refresh();
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnIsActiveChanged(bool value)
	{
		OnPropertyChanged("IsInactive");
		OnPropertyChanged("ActiveSince");
	}
}
