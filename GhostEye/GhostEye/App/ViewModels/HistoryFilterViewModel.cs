using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;

namespace GhostEye.App.ViewModels;

public sealed class HistoryFilterViewModel(string id, string labelKey) : ObservableObject, ILocalizedViewModel
{
	public string Id { get; } = id;

	public string Label => AppServices.Localizer[labelKey];

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
	}

	public void RefreshSelection()
	{
		OnPropertyChanged("IsSelected");
	}
}
