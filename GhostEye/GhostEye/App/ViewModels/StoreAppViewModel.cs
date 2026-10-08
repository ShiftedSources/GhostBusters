using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Apps;

namespace GhostEye.App.ViewModels;

public sealed class StoreAppViewModel : ObservableObject
{
	public StoreApp App { get; }

	public string Name => AppServices.Localizer["bloat." + App.Definition.Key + ".name"];

	public string Description => AppServices.Localizer[App.Definition.DescriptionKey];

	public string PackageName => App.Definition.PackageName;

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

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Error
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Error);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Error);
			}
		}
	}

	public StoreAppViewModel(StoreApp app)
	{
		App = app;
		Error = string.Empty;
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Name");
		OnPropertyChanged("Description");
	}
}
