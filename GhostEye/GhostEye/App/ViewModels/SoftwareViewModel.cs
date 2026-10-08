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
using GhostEye.Core.Presets;
using GhostEye.Infrastructure.Apps;

namespace GhostEye.App.ViewModels;

public sealed class SoftwareViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly WingetService _winget;

	private bool _loaded;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? installSelectedCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? checkUpgradesCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<UpgradeViewModel?>? upgradeOneCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? upgradeAllCommand;

	public ObservableCollection<InstallGroupViewModel> Groups { get; } = new ObservableCollection<InstallGroupViewModel>();

	public ObservableCollection<UpgradeViewModel> Upgrades { get; } = new ObservableCollection<UpgradeViewModel>();

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
	public bool IsAvailable
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsAvailable);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsAvailable);
			}
		}
	} = true;

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
	public string ProgressLine
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ProgressLine);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ProgressLine);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasCheckedUpgrades
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasCheckedUpgrades);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasCheckedUpgrades);
			}
		}
	}

	public bool HasUpgrades => Upgrades.Count > 0;

	public string UpgradesSummary => AppServices.Localizer.Format("software.upgrades.count", Upgrades.Count);

	private IEnumerable<InstallableViewModel> AllItems => Groups.SelectMany((InstallGroupViewModel g) => g.Items);

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(RefreshAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand InstallSelectedCommand => installSelectedCommand ?? (installSelectedCommand = new AsyncRelayCommand(InstallSelectedAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand CheckUpgradesCommand => checkUpgradesCommand ?? (checkUpgradesCommand = new AsyncRelayCommand(CheckUpgradesAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<UpgradeViewModel?> UpgradeOneCommand => upgradeOneCommand ?? (upgradeOneCommand = new AsyncRelayCommand<UpgradeViewModel>(UpgradeOneAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand UpgradeAllCommand => upgradeAllCommand ?? (upgradeAllCommand = new AsyncRelayCommand(UpgradeAllAsync));

	public SoftwareViewModel()
	{
		_winget = AppServices.Winget;
		StatusMessage = string.Empty;
		ProgressLine = string.Empty;
		List<InstallableViewModel> source = WingetCatalog.Packages.Select((WingetPackage p) => new InstallableViewModel(p)).ToList();
		foreach (string group in WingetCatalog.Groups)
		{
			Groups.Add(new InstallGroupViewModel(group, source.Where((InstallableViewModel i) => i.Package.Group == group).ToList()));
		}
	}

	public async Task ActivatedAsync()
	{
		if (!_loaded)
		{
			_loaded = true;
			await RefreshAsync();
		}
	}

	[RelayCommand]
	private async Task RefreshAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["software.loading"];
		try
		{
			Task<IReadOnlyList<InstalledApp>> desktopApps = AppServices.InstalledApps.GetDesktopAppsAsync();
			IsAvailable = await _winget.IsAvailableAsync();
			IReadOnlySet<string> readOnlySet = ((!IsAvailable) ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : (await _winget.GetInstalledIdsAsync()));
			IReadOnlySet<string> wingetIds = readOnlySet;
			IReadOnlyList<string> source;
			try
			{
				source = (await desktopApps).Select((InstalledApp a) => a.Name).ToList();
			}
			catch (Exception exception)
			{
				AppServices.Logger.Error("Reading installed programs for the Software page failed", exception);
				source = Array.Empty<string>();
			}
			foreach (InstallableViewModel allItem in AllItems)
			{
				allItem.IsInstalled = wingetIds.Contains(allItem.Id) || source.Any(allItem.Package.MatchesInstalledName);
				allItem.IsSelected = false;
			}
			StatusMessage = (IsAvailable ? string.Empty : AppServices.Localizer["software.unavailable"]);
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task InstallSelectedAsync()
	{
		List<InstallableViewModel> selected = AllItems.Where((InstallableViewModel i) => i.IsSelected && !i.IsInstalled).ToList();
		ILocalizer loc = AppServices.Localizer;
		if (selected.Count == 0)
		{
			StatusMessage = loc["software.nothingSelected"];
		}
		else
		{
			IsBusy = true;
			int done = 0;
			try
			{
				foreach (InstallableViewModel item in selected)
				{
					StatusMessage = loc.Format("software.installing", item.Name, done + 1, selected.Count);
					item.Status = loc["software.status.installing"];
					WingetResult wingetResult = await _winget.InstallAsync(item.Id, new Progress<string>((string line) =>
					{
						ProgressLine = line;
					}));
					item.Status = (wingetResult.Success ? loc["software.status.installed"] : wingetResult.Message);
					if (wingetResult.Success)
					{
						item.IsInstalled = true;
						item.IsSelected = false;
						done++;
					}
					AppServices.Logger.Info($"winget install {item.Id}: {(wingetResult.Success ? "ok" : "failed")} {wingetResult.Message}");
				}
				StatusMessage = loc.Format("software.installed", done, selected.Count);
			}
			finally
			{
				ProgressLine = string.Empty;
				IsBusy = false;
			}
		}
	}

	[RelayCommand]
	private async Task CheckUpgradesAsync()
	{
		IsBusy = true;
		StatusMessage = AppServices.Localizer["software.checking"];
		try
		{
			IReadOnlyList<WingetUpgrade> readOnlyList = await _winget.GetUpgradesAsync();
			Upgrades.Clear();
			foreach (WingetUpgrade item in readOnlyList)
			{
				Upgrades.Add(new UpgradeViewModel(item));
			}
			HasCheckedUpgrades = true;
			StatusMessage = ((Upgrades.Count == 0) ? AppServices.Localizer["software.upToDate"] : string.Empty);
			OnPropertyChanged("HasUpgrades");
			OnPropertyChanged("UpgradesSummary");
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task UpgradeOneAsync(UpgradeViewModel? item)
	{
		bool flag = item == null || !item.CanUpdateAlone;
		if (flag)
		{
			return;
		}
		ILocalizer loc = AppServices.Localizer;
		item.IsBusy = true;
		item.Status = loc["software.status.updating"];
		try
		{
			WingetResult wingetResult = await _winget.UpgradeAsync(item.Upgrade.Id, new Progress<string>((string line) =>
			{
				ProgressLine = line;
			}));
			item.Status = (wingetResult.Success ? loc["software.status.updated"] : wingetResult.Message);
			if (wingetResult.Success)
			{
				Upgrades.Remove(item);
				OnPropertyChanged("HasUpgrades");
				OnPropertyChanged("UpgradesSummary");
			}
			AppServices.Logger.Info($"winget upgrade {item.Upgrade.Id}: {(wingetResult.Success ? "ok" : "failed")} {wingetResult.Message}");
		}
		finally
		{
			item.IsBusy = false;
			ProgressLine = string.Empty;
		}
	}

	[RelayCommand]
	private async Task UpgradeAllAsync()
	{
		bool flag = Upgrades.Count == 0;
		if (flag)
		{
			return;
		}
		ILocalizer loc = AppServices.Localizer;
		if (!(await AppServices.Dialogs.ConfirmAsync(loc["software.upgradeAll.title"], loc.Format("software.upgradeAll.body", Upgrades.Count), loc["software.upgradeAll"])))
		{
			return;
		}
		IsBusy = true;
		StatusMessage = loc["software.updatingAll"];
		try
		{
			WingetResult wingetResult = await _winget.UpgradeAllAsync(new Progress<string>((string line) =>
			{
				ProgressLine = line;
			}));
			AppServices.Logger.Info("winget upgrade --all: " + (wingetResult.Success ? "ok" : "failed") + " " + wingetResult.Message);
		}
		finally
		{
			ProgressLine = string.Empty;
			IsBusy = false;
		}
		await CheckUpgradesAsync();
		StatusMessage = ((Upgrades.Count == 0) ? loc["software.upToDate"] : loc.Format("software.upgradesLeft", Upgrades.Count));
	}

	public void OnLanguageChanged()
	{
		foreach (InstallGroupViewModel item in Groups.ToList())
		{
			int index = Groups.IndexOf(item);
			Groups[index] = new InstallGroupViewModel(item.Key, item.Items);
		}
		StatusMessage = string.Empty;
		OnPropertyChanged("UpgradesSummary");
	}
}
