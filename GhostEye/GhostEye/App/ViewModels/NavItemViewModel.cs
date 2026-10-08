using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;

namespace GhostEye.App.ViewModels;

public sealed class NavItemViewModel : ObservableObject, ILocalizedViewModel
{
	public required string Id { get; init; }

	public required string LabelKey { get; init; }

	public required string IconKey { get; init; }

	public required string TitleKey { get; init; }

	public required string SubtitleKey { get; init; }

	public required Func<ObservableObject> Factory { get; init; }

	public ObservableObject? Instance { get; set; }

	public string Label => AppServices.Localizer[LabelKey];

	public string Title => AppServices.Localizer[TitleKey];

	public string Subtitle => AppServices.Localizer[SubtitleKey];

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsSelected
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsSelected);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsSelected);
			}
		}
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Label");
		OnPropertyChanged("Title");
		OnPropertyChanged("Subtitle");
	}
}
