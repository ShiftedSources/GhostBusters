using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class StartupItemViewModel : ObservableObject
{
	public StartupEntry Entry { get; }

	public string Name
	{
		get
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(Entry.Name);
			bool flag = fileNameWithoutExtension != null && fileNameWithoutExtension.Length > 0;
			if (flag)
			{
				StartupSource source = Entry.Source;
				bool flag2 = (((uint)(source - 3) <= 1u || (uint)(source - 9) <= 1u) ? true : false);
				flag = flag2;
			}
			if (!flag)
			{
				return Entry.Name;
			}
			return fileNameWithoutExtension;
		}
	}

	public string Command => Entry.Command;

	public string Location => Entry.Location;

	public ImageSource? Icon { get; }

	public bool HasIcon => Icon != null;

	public string Publisher { get; }

	public bool HasPublisher => Publisher.Length > 0;

	public bool FileExists { get; }

	public bool IsOrphan
	{
		get
		{
			if (!FileExists && !Entry.IsRemoved)
			{
				return !string.IsNullOrWhiteSpace(Entry.Command);
			}
			return false;
		}
	}

	public bool IsRemoved => Entry.IsRemoved;

	public bool IsMicrosoft => Publisher.Contains("Microsoft", StringComparison.OrdinalIgnoreCase);

	public bool CanToggle => !Entry.IsRemoved;

	public bool CanRemove
	{
		get
		{
			StartupSource source = Entry.Source;
			if ((uint)source <= 4u)
			{
				return true;
			}
			return false;
		}
	}

	public bool CanOpenLocation => FileExists;

	public string SourceLabel => AppServices.Localizer[$"startup.source.{Entry.Source}"];

	public string InfoText => AppServices.Localizer.Format("startup.info.body", SourceLabel, Location, Command);

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsOn
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsOn);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsOn);
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

	public StartupItemViewModel(StartupEntry entry)
	{
		Entry = entry;
		IsOn = entry.IsEnabled;
		Error = string.Empty;
		string executablePath = entry.ExecutablePath;
		FileExists = executablePath != null && File.Exists(executablePath);
		Icon = (FileExists ? FileIcons.For(executablePath) : null);
		Publisher = (FileExists ? ReadPublisher(executablePath) : string.Empty);
	}

	public void ResetSwitch()
	{
		OnPropertyChanged("IsOn");
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("SourceLabel");
		OnPropertyChanged("InfoText");
	}

	private static string ReadPublisher(string path)
	{
		try
		{
			return FileVersionInfo.GetVersionInfo(path).CompanyName?.Trim() ?? string.Empty;
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException) ? true : false)
		{
			return string.Empty;
		}
	}
}
