using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class PingRowViewModel : ObservableObject
{
	public PingTarget Target { get; }

	public string Name => AppServices.Localizer["network.ping." + Target.Key];

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
	public RiskLevel Quality
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<RiskLevel>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Quality);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Quality);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool HasResult
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.HasResult);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.HasResult);
			}
		}
	}

	public PingRowViewModel(PingTarget target)
	{
		Target = target;
		Value = "—";
		Detail = string.Empty;
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Name");
	}
}
