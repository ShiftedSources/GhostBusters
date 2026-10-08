using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Disk;

namespace GhostEye.App.ViewModels;

public sealed class DuplicateFileViewModel : ObservableObject
{
	public DuplicateFile File { get; }

	public string Name => Path.GetFileName(File.Path);

	public string Folder => Path.GetDirectoryName(File.Path) ?? string.Empty;

	public string DateLabel => File.LastWrite.ToString("g", AppServices.Localizer.Culture);

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

	public DuplicateFileViewModel(DuplicateFile file, bool keep)
	{
		File = file;
		IsSelected = !keep;
	}
}
