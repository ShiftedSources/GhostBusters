using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Cleanup;

namespace GhostEye.App.ViewModels;

public sealed class CleanupCategoryViewModel : ObservableObject
{
	public CleanupCategory Category { get; }

	public string Name => Category.Name;

	public string Description => Category.Description;

	public string SizeLabel => GhostFormat.Bytes(Category.Bytes);

	public string FileCountLabel => AppServices.Localizer.Format("cleanup.files", Category.FileCount);

	public string? Warning => Category.Warning;

	public bool HasWarning => !string.IsNullOrWhiteSpace(Category.Warning);

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

	public CleanupCategoryViewModel(CleanupCategory category)
	{
		Category = category;
		IsSelected = !CleanupScanner.IsOptional(category.Id);
	}
}
