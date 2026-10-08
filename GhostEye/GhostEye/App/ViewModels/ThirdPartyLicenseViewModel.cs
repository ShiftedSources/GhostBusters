using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;

namespace GhostEye.App.ViewModels;

public sealed class ThirdPartyLicenseViewModel : ObservableObject
{
	private readonly string[] _files;

	private string? _text;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? toggleCommand;

	public string Name { get; }

	public string License { get; }

	public string Url { get; }

	public string Text => _text ?? (_text = string.Join(Environment.NewLine + Environment.NewLine + new string('-', 60) + Environment.NewLine + Environment.NewLine, _files.Select(Read)));

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsExpanded
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsExpanded);
				field = value;
				OnIsExpandedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsExpanded);
			}
		}
	}

	public static IReadOnlyList<ThirdPartyLicenseViewModel> All { get; } = new _003C_003Ez__ReadOnlyArray<ThirdPartyLicenseViewModel>(new ThirdPartyLicenseViewModel[8]
	{
		new ThirdPartyLicenseViewModel("Inter", "SIL Open Font License 1.1", "https://github.com/rsms/inter", "OFL-Inter.txt"),
		new ThirdPartyLicenseViewModel("Space Grotesk", "SIL Open Font License 1.1", "https://github.com/floriankarsten/space-grotesk", "OFL-SpaceGrotesk.txt"),
		new ThirdPartyLicenseViewModel("JetBrains Mono", "SIL Open Font License 1.1", "https://github.com/JetBrains/JetBrainsMono", "OFL-JetBrainsMono.txt"),
		new ThirdPartyLicenseViewModel(".NET Runtime", "MIT", "https://github.com/dotnet/runtime", "DotNet-LICENSE.txt", "DotNet-THIRD-PARTY-NOTICES.txt"),
		new ThirdPartyLicenseViewModel("WPF (Windows Presentation Foundation)", "MIT", "https://github.com/dotnet/wpf", "WPF-LICENSE.txt", "WPF-THIRD-PARTY-NOTICES.txt"),
		new ThirdPartyLicenseViewModel(".NET Community Toolkit (MVVM)", "MIT", "https://github.com/CommunityToolkit/dotnet", "CommunityToolkit-LICENSE.txt", "CommunityToolkit-THIRD-PARTY-NOTICES.txt"),
		new ThirdPartyLicenseViewModel("Windows Forms (notification-area icon)", "MIT", "https://github.com/dotnet/winforms", "WinForms-LICENSE.txt"),
		new ThirdPartyLicenseViewModel("Intel PresentMon 2.6.0 (FPS measurement)", "MIT", "https://github.com/GameTechDev/PresentMon", "MIT-PresentMon.txt")
	});

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ToggleCommand => toggleCommand ?? (toggleCommand = new RelayCommand(Toggle));

	public ThirdPartyLicenseViewModel(string name, string license, string url, params string[] files)
	{
		Name = name;
		License = license;
		Url = url;
		_files = files;
	}

	[RelayCommand]
	private void Toggle()
	{
		IsExpanded = !IsExpanded;
	}

	private static string Read(string file)
	{
		try
		{
			StreamResourceInfo resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/Licenses/" + file));
			if (resourceStream == null)
			{
				return file;
			}
			using StreamReader streamReader = new StreamReader(resourceStream.Stream);
			return streamReader.ReadToEnd().Trim();
		}
		catch (IOException)
		{
			return file;
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnIsExpandedChanged(bool value)
	{
		OnPropertyChanged("Text");
	}
}
