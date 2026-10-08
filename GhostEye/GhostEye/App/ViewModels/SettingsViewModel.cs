using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;
using Microsoft.Win32;

namespace GhostEye.App.ViewModels;

public sealed class SettingsViewModel : ObservableObject, ILocalizedViewModel
{
	private const string RunKey = "HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run";

	private const string RunValueName = "GhostEye";

	private readonly AppSettingsService _settings;

	private bool _applyingLanguage;

	private bool _loadingMaintenance;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<object?>? setMaintenanceIntervalCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? runMaintenanceNowCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? exportProfileCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? importProfileCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? openWebsiteCommand;

	public ObservableCollection<LanguageOptionViewModel> LanguageOptions { get; } = new ObservableCollection<LanguageOptionViewModel>();

	public IReadOnlyList<ThirdPartyLicenseViewModel> ThirdPartyLicenses => ThirdPartyLicenseViewModel.All;

	public string Version => AppServices.Localizer.Format("app.version", AppInfo.Version);

	public string Developer => "@SoloShown";

	public string Tagline => AppServices.Localizer["app.tagline"];

	public static bool StartupManagedByWindows => PackageContext.IsPackaged;

	public string StartupManagedHint => AppServices.Localizer["settings.startWithWindows.packaged"];

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool SnowEnabled
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SnowEnabled);
				field = value;
				OnSnowEnabledChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SnowEnabled);
			}
		}
	}

	public bool SnowAutoOff
	{
		get
		{
			if (SnowEnabled)
			{
				return !RenderBudget.AllowsAnimations;
			}
			return false;
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool NotificationsEnabled
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.NotificationsEnabled);
				field = value;
				OnNotificationsEnabledChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.NotificationsEnabled);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool AutoRestorePoint
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.AutoRestorePoint);
				field = value;
				OnAutoRestorePointChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.AutoRestorePoint);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool StartWithWindows
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.StartWithWindows);
				field = value;
				OnStartWithWindowsChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.StartWithWindows);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool RunInBackground
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RunInBackground);
				field = value;
				OnRunInBackgroundChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RunInBackground);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool MaintenanceEnabled
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.MaintenanceEnabled);
				field = value;
				OnMaintenanceEnabledChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.MaintenanceEnabled);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public int MaintenanceIntervalDays
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.MaintenanceIntervalDays);
				field = value;
				OnMaintenanceIntervalDaysChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.MaintenanceIntervalDays);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string MaintenanceMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.MaintenanceMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.MaintenanceMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsMaintenanceBusy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsMaintenanceBusy);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsMaintenanceBusy);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string ProfileMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ProfileMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ProfileMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsProfileBusy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsProfileBusy);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsProfileBusy);
			}
		}
	}

	public string LastMaintenanceLabel
	{
		get
		{
			DateTimeOffset? lastMaintenance = _settings.Current.LastMaintenance;
			if (lastMaintenance.HasValue)
			{
				DateTimeOffset valueOrDefault = lastMaintenance.GetValueOrDefault();
				return AppServices.Localizer.Format("maintenance.last", valueOrDefault.LocalDateTime.ToString("g", AppServices.Localizer.Culture), GhostFormat.Bytes(_settings.Current.LastMaintenanceFreedBytes));
			}
			return AppServices.Localizer["maintenance.never"];
		}
	}

	public static string Website => "ghosteye.store";

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<object?> SetMaintenanceIntervalCommand => setMaintenanceIntervalCommand ?? (setMaintenanceIntervalCommand = new RelayCommand<object>(SetMaintenanceInterval));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RunMaintenanceNowCommand => runMaintenanceNowCommand ?? (runMaintenanceNowCommand = new AsyncRelayCommand(RunMaintenanceNowAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ExportProfileCommand => exportProfileCommand ?? (exportProfileCommand = new AsyncRelayCommand(ExportProfileAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ImportProfileCommand => importProfileCommand ?? (importProfileCommand = new AsyncRelayCommand(ImportProfileAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand OpenWebsiteCommand => openWebsiteCommand ?? (openWebsiteCommand = new RelayCommand(OpenWebsite));

	public SettingsViewModel()
	{
		_settings = AppServices.Settings;
		AppSettings current = _settings.Current;
		foreach (var item3 in Languages.All)
		{
			string item = item3.Code;
			string item2 = item3.DisplayName;
			LanguageOptionViewModel option = new LanguageOptionViewModel(item, item2)
			{
				IsSelected = (item == AppServices.Localizer.Language)
			};
			option.PropertyChanged += (object? _, PropertyChangedEventArgs e) =>
			{
				if (!(e.PropertyName != "IsSelected"))
				{
					if (option.IsSelected)
					{
						SelectLanguage(option.Code);
					}
					else if (!_applyingLanguage && option.Code == AppServices.Localizer.Language)
					{
						Application.Current?.Dispatcher.BeginInvoke((Func<bool>)(() => option.IsSelected = true));
					}
				}
			};
			LanguageOptions.Add(option);
		}
		SnowEnabled = current.SnowEnabled;
		RenderBudget.Changed += (object? _, EventArgs _) =>
		{
			OnPropertyChanged("SnowAutoOff");
		};
		NotificationsEnabled = current.NotificationsEnabled;
		AutoRestorePoint = current.AutoRestorePoint;
		StartWithWindows = current.StartWithWindows;
		RunInBackground = current.RunInBackground;
		_loadingMaintenance = true;
		MaintenanceEnabled = current.MaintenanceEnabled;
		_loadingMaintenance = false;
		MaintenanceIntervalDays = current.MaintenanceIntervalDays;
		MaintenanceMessage = string.Empty;
		ProfileMessage = string.Empty;
	}

	[RelayCommand]
	private void SetMaintenanceInterval(object? days)
	{
		if (days != null && int.TryParse(days.ToString(), out var result))
		{
			MaintenanceIntervalDays = result;
		}
	}

	[RelayCommand]
	private async Task RunMaintenanceNowAsync()
	{
		IsMaintenanceBusy = true;
		MaintenanceMessage = AppServices.Localizer["maintenance.running"];
		try
		{
			MaintenanceReport maintenanceReport = await BackgroundAgent.RunMaintenanceAsync();
			MaintenanceMessage = ((maintenanceReport.DriftedTweaks > 0) ? AppServices.Localizer.Format("maintenance.notify.bodyDrift", GhostFormat.Bytes(maintenanceReport.FreedBytes), maintenanceReport.DriftedTweaks) : AppServices.Localizer.Format("maintenance.notify.body", GhostFormat.Bytes(maintenanceReport.FreedBytes)));
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Manual maintenance failed", ex);
			MaintenanceMessage = ex.Message;
		}
		finally
		{
			IsMaintenanceBusy = false;
			OnPropertyChanged("LastMaintenanceLabel");
		}
	}

	[RelayCommand]
	private async Task ExportProfileAsync()
	{
		ILocalizer loc = AppServices.Localizer;
		SaveFileDialog dialog = new SaveFileDialog
		{
			Title = loc["profile.export.title"],
			Filter = loc["profile.filter"],
			FileName = "GhostEye-" + Environment.MachineName + ".ghostprofile.json"
		};
		if (dialog.ShowDialog() != true)
		{
			return;
		}
		IsProfileBusy = true;
		try
		{
			OptimizationProfile optimizationProfile = await AppServices.Profiles.CaptureCurrentAsync(Environment.MachineName);
			AppServices.Profiles.Save(optimizationProfile, dialog.FileName);
			ProfileMessage = loc.Format("profile.exported", optimizationProfile.Tweaks.Count, optimizationProfile.Services.Count);
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			ProfileMessage = ex.Message;
		}
		finally
		{
			IsProfileBusy = false;
		}
	}

	[RelayCommand]
	private async Task ImportProfileAsync()
	{
		ILocalizer loc = AppServices.Localizer;
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Title = loc["profile.import.title"],
			Filter = loc["profile.filter"],
			CheckFileExists = true
		};
		if (openFileDialog.ShowDialog() != true)
		{
			return;
		}
		OptimizationProfile profile = AppServices.Profiles.Load(openFileDialog.FileName, out string error);
		if (error.Length > 0)
		{
			ProfileMessage = error;
			return;
		}
		IEnumerable<string> first = from id in profile.Tweaks
			select TweakCatalog.Find(id) into d
			where (object)d != null
			select "· " + loc[d.NameKey];
		IEnumerable<string> second = profile.Services.Select((string n) => "· " + loc["svc." + n + ".name"]);
		bool flag = !(await AppServices.Dialogs.ConfirmAsync(loc["profile.import.confirmTitle"], loc.Format("profile.import.confirmBody", string.IsNullOrWhiteSpace(profile.Name) ? "—" : profile.Name, profile.Tweaks.Count, profile.Services.Count) + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, first.Concat(second).Take(30)), loc["common.apply"]));
		if (!flag)
		{
			flag = !(await AdminGate.RequireAdminAsync("settings"));
		}
		if (flag)
		{
			return;
		}
		IsProfileBusy = true;
		ProfileMessage = loc["profile.applying"];
		try
		{
			ProfileApplyReport profileApplyReport = await AppServices.Profiles.ApplyAsync(profile, new Progress<string>((string step) =>
			{
				ProfileMessage = step;
			}));
			ProfileMessage = (profileApplyReport.Success ? loc.Format("profile.applied", profileApplyReport.TweaksApplied, profileApplyReport.ServicesApplied) : loc.Format("profile.appliedPartial", profileApplyReport.TweaksApplied, profileApplyReport.ServicesApplied, profileApplyReport.TweaksFailed + profileApplyReport.ServicesFailed));
		}
		finally
		{
			IsProfileBusy = false;
		}
	}

	[RelayCommand]
	private static void OpenWebsite()
	{
		WebLinks.OpenWebsite();
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("LastMaintenanceLabel");
		MaintenanceMessage = string.Empty;
		ProfileMessage = string.Empty;
		OnPropertyChanged("Version");
		OnPropertyChanged("Tagline");
	}

	private void SelectLanguage(string code)
	{
		if (_applyingLanguage || code == AppServices.Localizer.Language)
		{
			return;
		}
		_applyingLanguage = true;
		try
		{
			foreach (LanguageOptionViewModel languageOption in LanguageOptions)
			{
				languageOption.IsSelected = languageOption.Code == code;
			}
			_settings.Update((AppSettings s) => s with
			{
				Language = code
			});
			AppServices.SetLanguage(code);
		}
		finally
		{
			_applyingLanguage = false;
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnSnowEnabledChanged(bool value)
	{
		_settings.Update((AppSettings s) => s with
		{
			SnowEnabled = value
		});
		OnPropertyChanged("SnowAutoOff");
		(Application.Current?.MainWindow as ShellWindow)?.SetSnowEnabled(value);
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnNotificationsEnabledChanged(bool value)
	{
		_settings.Update((AppSettings s) => s with
		{
			NotificationsEnabled = value
		});
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnAutoRestorePointChanged(bool value)
	{
		_settings.Update((AppSettings s) => s with
		{
			AutoRestorePoint = value
		});
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnStartWithWindowsChanged(bool value)
	{
		_settings.Update((AppSettings s) => s with
		{
			StartWithWindows = value
		});
		if (PackageContext.IsPackaged)
		{
			return;
		}
		try
		{
			string processPath = Environment.ProcessPath;
			if (value && !string.IsNullOrEmpty(processPath))
			{
				string text = (_settings.Current.RunInBackground ? " --tray" : string.Empty);
				AppServices.Registry.WriteString("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "GhostEye", "\"" + processPath + "\"" + text);
			}
			else if (!value)
			{
				AppServices.Registry.DeleteValue("HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run", "GhostEye");
			}
		}
		catch (Exception ex) when ((ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException) ? true : false)
		{
			AppServices.Logger.Error("Could not update the GhostEye startup entry", ex);
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnRunInBackgroundChanged(bool value)
	{
		_settings.Update((AppSettings s) => s with
		{
			RunInBackground = value
		});
		if (StartWithWindows)
		{
			OnStartWithWindowsChanged(value: true);
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnMaintenanceEnabledChanged(bool value)
	{
		if (_loadingMaintenance)
		{
			return;
		}
		_settings.Update((AppSettings s) => s with
		{
			MaintenanceEnabled = value
		});
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnMaintenanceIntervalDaysChanged(int value)
	{
		_settings.Update((AppSettings s) => s with
		{
			MaintenanceIntervalDays = Math.Clamp(value, 1, 90)
		});
	}
}
