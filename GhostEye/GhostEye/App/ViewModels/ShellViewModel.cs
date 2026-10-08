using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;

namespace GhostEye.App.ViewModels;

public sealed class ShellViewModel : ObservableObject
{
	private bool _navigating;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? openWebsiteCommand;

	public DialogHostViewModel Dialogs { get; }

	public ObservableCollection<NavItemViewModel> Items { get; } = new ObservableCollection<NavItemViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public ObservableObject? CurrentPage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<ObservableObject>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CurrentPage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CurrentPage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Title
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Title);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Title);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Subtitle
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Subtitle);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Subtitle);
			}
		}
	}

	public string StatusLine => $"{OsLabel()} · {AppServices.Localizer[IsElevated ? "app.status.admin" : "app.status.user"]} · v{AppInfo.Version}";

	public string Website => "ghosteye.store";

	public string ElevationBadge => AppServices.Localizer[IsElevated ? "app.badge.elevated" : "app.badge.limited"];

	public string BrandSubtitle => AppServices.Localizer["app.subtitle"];

	public static bool IsElevated { get; } = DetectElevation();

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand OpenWebsiteCommand => openWebsiteCommand ?? (openWebsiteCommand = new RelayCommand(OpenWebsite));

	public ShellViewModel()
	{
		Dialogs = new DialogHostViewModel();
		Title = string.Empty;
		Subtitle = string.Empty;
		BuildNavigation();
		foreach (NavItemViewModel item in Items)
		{
			item.PropertyChanged += OnNavItemChanged;
		}
		Navigate(Items[0]);
	}

	private void OnNavItemChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (!_navigating && !(e.PropertyName != "IsSelected") && sender is NavItemViewModel { IsSelected: not false } navItemViewModel)
		{
			Navigate(navItemViewModel);
		}
	}

	[RelayCommand]
	private static void OpenWebsite()
	{
		WebLinks.OpenWebsite();
	}

	private void BuildNavigation()
	{
		Add("dashboard", "Icon.Dashboard", () => new DashboardViewModel());
		Add("analyzer", "Icon.Analyzer", () => new AnalyzerViewModel());
		Add("optimize", "Icon.Optimize", () => new OptimizeViewModel());
		Add("gaming", "Icon.Gaming", () => new GamingViewModel());
		Add("network", "Icon.Network", () => new NetworkPageViewModel());
		Add("privacy", "Icon.Privacy", () => new PrivacyViewModel());
		Add("interface", "Icon.Interface", () => new InterfaceViewModel());
		Add("startup", "Icon.Startup", () => new StartupViewModel());
		Add("apps", "Icon.Apps", () => new AppsViewModel());
		Add("software", "Icon.Software", () => new SoftwareViewModel());
		Add("services", "Icon.Services", () => new ServicesViewModel());
		Add("cleanup", "Icon.Cleanup", () => new CleanupViewModel());
		Add("disk", "Icon.Disk", () => new DiskViewModel());
		Add("health", "Icon.Health", () => new HealthViewModel());
		Add("tools", "Icon.Tools", () => new ToolsViewModel());
		Add("history", "Icon.History", () => new HistoryViewModel());
		Add("settings", "Icon.Settings", () => new SettingsViewModel());
		void Add(string id, string icon, Func<ObservableObject> factory)
		{
			Items.Add(new NavItemViewModel
			{
				Id = id,
				LabelKey = "nav." + id,
				IconKey = icon,
				TitleKey = "page." + id + ".title",
				SubtitleKey = "page." + id + ".subtitle",
				Factory = factory
			});
		}
	}

	public void Navigate(NavItemViewModel item)
	{
		if (_navigating)
		{
			return;
		}
		_navigating = true;
		try
		{
			foreach (NavItemViewModel item2 in Items)
			{
				item2.IsSelected = item2 == item;
			}
		}
		finally
		{
			_navigating = false;
		}
		if (item.Instance == null)
		{
			item.Instance = item.Factory();
		}
		Title = item.Title;
		Subtitle = item.Subtitle;
		CurrentPage = item.Instance;
		if (item.Instance is IPageViewModel pageViewModel)
		{
			pageViewModel.ActivatedAsync();
		}
	}

	public void NavigateTo(string id)
	{
		NavItemViewModel navItemViewModel = Items.FirstOrDefault((NavItemViewModel i) => i.Id == id);
		if (navItemViewModel != null)
		{
			Navigate(navItemViewModel);
		}
	}

	public T? Find<T>() where T : class
	{
		return Items.Select((NavItemViewModel i) => i.Instance).OfType<T>().FirstOrDefault();
	}

	public void OnLanguageChanged()
	{
		foreach (NavItemViewModel item in Items)
		{
			item.OnLanguageChanged();
			if (item.Instance is ILocalizedViewModel localizedViewModel)
			{
				localizedViewModel.OnLanguageChanged();
			}
		}
		NavItemViewModel navItemViewModel = Items.FirstOrDefault((NavItemViewModel i) => i.IsSelected);
		if (navItemViewModel != null)
		{
			Title = navItemViewModel.Title;
			Subtitle = navItemViewModel.Subtitle;
		}
		OnPropertyChanged("StatusLine");
		OnPropertyChanged("ElevationBadge");
		OnPropertyChanged("BrandSubtitle");
	}

	private static bool DetectElevation()
	{
		using WindowsIdentity ntIdentity = WindowsIdentity.GetCurrent();
		return new WindowsPrincipal(ntIdentity).IsInRole(WindowsBuiltInRole.Administrator);
	}

	private static string OsLabel()
	{
		if (Environment.OSVersion.Version.Build < 22000)
		{
			return "WIN 10";
		}
		return "WIN 11";
	}
}
