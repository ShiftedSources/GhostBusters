using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class TweakItemViewModel : ObservableObject, ILocalizedViewModel
{
	public TweakDefinition Definition { get; }

	public string Id => Definition.Id;

	public string Name => AppServices.Localizer[Definition.NameKey];

	public string Description => AppServices.Localizer[Definition.DescriptionKey];

	public string Details
	{
		get
		{
			string text = "tweak." + Definition.Id + ".details";
			string text2 = AppServices.Localizer[text];
			if (!(text2 == text))
			{
				return text2;
			}
			return string.Empty;
		}
	}

	public string InfoFooter
	{
		get
		{
			ILocalizer localizer = AppServices.Localizer;
			List<string> list = new List<string>
			{
				RequiresRestart ? localizer["info.restart"] : localizer["info.noRestart"],
				localizer["info.reversible"]
			};
			if (IsAdvancedOnly)
			{
				list.Add(localizer["info.advanced"]);
			}
			return string.Join(" ", list);
		}
	}

	public RiskLevel Risk => Definition.Risk;

	public bool RequiresRestart => Definition.RequiresRestart;

	public bool IsAdvancedOnly => Definition.AdvancedOnly;

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
	public bool IsApplicable
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsApplicable);
				field = value;
				OnIsApplicableChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsApplicable);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string UnavailableReason
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.UnavailableReason);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.UnavailableReason);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string? CurrentValue
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CurrentValue);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CurrentValue);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsAlreadyApplied
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsAlreadyApplied);
				field = value;
				OnIsAlreadyAppliedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsAlreadyApplied);
			}
		}
	}

	public string StateLabel
	{
		get
		{
			if (IsApplicable)
			{
				if (!IsAlreadyApplied)
				{
					return string.Empty;
				}
				return AppServices.Localizer["state.alreadyOn"];
			}
			return AppServices.Localizer["state.notApplicable"];
		}
	}

	public bool HasStateLabel => StateLabel.Length > 0;

	public TweakItemViewModel(TweakDefinition definition)
	{
		Definition = definition;
		IsApplicable = true;
		UnavailableReason = string.Empty;
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Name");
		OnPropertyChanged("Description");
		OnPropertyChanged("Details");
		OnPropertyChanged("InfoFooter");
		RefreshState();
	}

	private void RefreshState()
	{
		OnPropertyChanged("StateLabel");
		OnPropertyChanged("HasStateLabel");
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnIsApplicableChanged(bool value)
	{
		RefreshState();
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnIsAlreadyAppliedChanged(bool value)
	{
		RefreshState();
	}
}
