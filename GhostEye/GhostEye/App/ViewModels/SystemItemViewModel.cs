using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class SystemItemViewModel : ObservableObject
{
	private readonly string _nameKey;

	private readonly string _descriptionKey;

	public string Id { get; }

	public string TechnicalName { get; }

	public RiskLevel Risk { get; }

	public string Name => AppServices.Localizer[_nameKey];

	public string Description => AppServices.Localizer[_descriptionKey];

	public string Details
	{
		get
		{
			string text = _descriptionKey.Replace(".desc", ".details", StringComparison.Ordinal);
			string text2 = AppServices.Localizer[text];
			if (!(text2 == text))
			{
				return text2;
			}
			return string.Empty;
		}
	}

	public ICommand? ToggleCommand { get; init; }

	public string InfoFooter => AppServices.Localizer.Format("services.info.footer", TechnicalName);

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsOptimized
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsOptimized);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsOptimized);
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

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string StateLabel
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.StateLabel);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.StateLabel);
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

	public SystemItemViewModel(string id, string nameKey, string descriptionKey, RiskLevel risk, string technicalName)
	{
		Id = id;
		_nameKey = nameKey;
		_descriptionKey = descriptionKey;
		Risk = risk;
		TechnicalName = technicalName;
		Error = string.Empty;
		StateLabel = string.Empty;
	}

	public void ResetSwitch()
	{
		OnPropertyChanged("IsOptimized");
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Name");
		OnPropertyChanged("Description");
		OnPropertyChanged("Details");
		OnPropertyChanged("InfoFooter");
	}
}
