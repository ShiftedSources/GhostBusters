using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Games;
using Microsoft.Win32;

namespace GhostEye.App.ViewModels;

public sealed class AutoBoostViewModel : ObservableObject
{
	private bool _loading;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? refreshCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? addGameCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<GameItemViewModel?>? removeGameCommand;

	public ObservableCollection<GameItemViewModel> Games { get; } = new ObservableCollection<GameItemViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsEnabled
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsEnabled);
				field = value;
				OnIsEnabledChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsEnabled);
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

	public bool HasGames => Games.Count > 0;

	public string GamesSummary => AppServices.Localizer.Format("autoBoost.found", Games.Count);

	public bool ShowBackgroundHint
	{
		get
		{
			if (IsEnabled)
			{
				return !AppServices.Settings.Current.RunInBackground;
			}
			return false;
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new RelayCommand(Refresh));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand AddGameCommand => addGameCommand ?? (addGameCommand = new RelayCommand(AddGame));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<GameItemViewModel?> RemoveGameCommand => removeGameCommand ?? (removeGameCommand = new RelayCommand<GameItemViewModel>(RemoveGame));

	public AutoBoostViewModel()
	{
		_loading = true;
		IsEnabled = AppServices.Settings.Current.AutoBoostEnabled;
		_loading = false;
		StatusMessage = string.Empty;
	}

	[RelayCommand]
	public void Refresh()
	{
		IReadOnlyList<string> autoBoostExcludedGames = AppServices.Settings.Current.AutoBoostExcludedGames;
		Games.Clear();
		foreach (GameEntry item in BackgroundAgent.GetLibrary(refresh: true))
		{
			Games.Add(new GameItemViewModel(item, !autoBoostExcludedGames.Contains(item.Id, StringComparer.OrdinalIgnoreCase), OnGameToggled));
		}
		OnPropertyChanged("HasGames");
		OnPropertyChanged("GamesSummary");
		OnPropertyChanged("ShowBackgroundHint");
	}

	private static void OnGameToggled(GameItemViewModel item)
	{
		AppServices.Settings.Update((AppSettings s) => s with
		{
			AutoBoostExcludedGames = (item.IsIncluded ? s.AutoBoostExcludedGames.Where((string id) => !id.Equals(item.Game.Id, StringComparison.OrdinalIgnoreCase)).ToList() : s.AutoBoostExcludedGames.Append(item.Game.Id).Distinct(StringComparer.OrdinalIgnoreCase).ToList())
		});
	}

	[RelayCommand]
	private void AddGame()
	{
		OpenFileDialog dialog = new OpenFileDialog
		{
			Title = AppServices.Localizer["autoBoost.add.title"],
			Filter = AppServices.Localizer["autoBoost.add.filter"],
			CheckFileExists = true
		};
		if (dialog.ShowDialog() == true)
		{
			AppServices.Settings.Update((AppSettings s) => s with
			{
				CustomGames = s.CustomGames.Append(dialog.FileName).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
			});
			Refresh();
		}
	}

	[RelayCommand]
	private void RemoveGame(GameItemViewModel? item)
	{
		if (item == null || !item.IsCustom)
		{
			return;
		}
		AppServices.Settings.Update((AppSettings s) => s with
		{
			CustomGames = s.CustomGames.Where((string p) => !p.Equals(item.Game.InstallPath, StringComparison.OrdinalIgnoreCase)).ToList()
		});
		Refresh();
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("GamesSummary");
		StatusMessage = string.Empty;
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnIsEnabledChanged(bool value)
	{
		if (_loading)
		{
			return;
		}
		AppServices.Settings.Update((AppSettings s) => s with
		{
			AutoBoostEnabled = value
		});
		OnPropertyChanged("ShowBackgroundHint");
		if (value && Games.Count == 0)
		{
			Refresh();
		}
	}
}
