using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.Infrastructure.Games;

namespace GhostEye.App.ViewModels;

public sealed class GameItemViewModel : ObservableObject
{
	private readonly Action<GameItemViewModel> _changed;

	private bool _loading = true;

	public GameEntry Game { get; }

	public string Name => Game.Name;

	public string Launcher => Game.Launcher;

	public bool IsCustom => Game.Launcher == "Custom";

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsIncluded
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsIncluded);
				field = value;
				OnIsIncludedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsIncluded);
			}
		}
	}

	public GameItemViewModel(GameEntry game, bool included, Action<GameItemViewModel> changed)
	{
		Game = game;
		_changed = changed;
		IsIncluded = included;
		_loading = false;
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnIsIncludedChanged(bool value)
	{
		if (!_loading)
		{
			_changed(this);
		}
	}
}
