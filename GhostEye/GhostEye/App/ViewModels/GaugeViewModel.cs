using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;

namespace GhostEye.App.ViewModels;

public sealed class GaugeViewModel : ObservableObject
{
	private readonly string _labelKey;

	public string Label => AppServices.Localizer[_labelKey];

	[ObservableProperty]
	[NotifyPropertyChangedFor("Ratio")]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public double Percent
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<double>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Percent);
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Ratio);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Percent);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Ratio);
			}
		}
	}

	public double Ratio => Math.Clamp(Percent / 100.0, 0.0, 1.0);

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Value
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Value);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Value);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Detail
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Detail);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Detail);
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
	}

	public GaugeViewModel(string labelKey)
	{
		_labelKey = labelKey;
		Value = "—";
		Detail = string.Empty;
		IsAvailable = true;
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Label");
	}
}
