using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Apps;

namespace GhostEye.App.ViewModels;

public sealed class InstalledAppViewModel : ObservableObject
{
	public static readonly TimeSpan UnusedAfter = TimeSpan.FromDays(90.0);

	public InstalledApp App { get; }

	public string Name => App.Name;

	public string Publisher => App.Publisher;

	public ImageSource? Icon { get; }

	public bool HasIcon => Icon != null;

	public long SizeBytes => App.SizeBytes;

	public string SizeLabel
	{
		get
		{
			if (App.SizeBytes <= 0)
			{
				return "—";
			}
			return GhostFormat.Bytes(App.SizeBytes);
		}
	}

	public bool IsUnused
	{
		get
		{
			DateTime? lastUsed = App.LastUsed;
			if (lastUsed.HasValue)
			{
				DateTime valueOrDefault = lastUsed.GetValueOrDefault();
				return DateTime.Now - valueOrDefault > UnusedAfter;
			}
			return false;
		}
	}

	public DateTime LastUsedSort => App.LastUsed ?? DateTime.MaxValue;

	public string DetailLabel
	{
		get
		{
			ILocalizer localizer = AppServices.Localizer;
			List<string> list = new List<string>();
			if (!string.IsNullOrEmpty(App.Version))
			{
				list.Add("v" + App.Version);
			}
			DateTime? installedOn = App.InstalledOn;
			if (installedOn.HasValue)
			{
				DateTime valueOrDefault = installedOn.GetValueOrDefault();
				list.Add(localizer.Format("apps.installedOn", valueOrDefault.ToString("d", localizer.Culture)));
			}
			installedOn = App.LastUsed;
			string item;
			if (installedOn.HasValue)
			{
				DateTime valueOrDefault2 = installedOn.GetValueOrDefault();
				item = localizer.Format("apps.lastUsed", valueOrDefault2.ToString("d", localizer.Culture));
			}
			else
			{
				item = localizer["apps.lastUsedUnknown"];
			}
			list.Add(item);
			return string.Join(" · ", list);
		}
	}

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

	public InstalledAppViewModel(InstalledApp app)
	{
		App = app;
		Icon = FileIcons.For(app.IconPath);
		Error = string.Empty;
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("DetailLabel");
	}
}
