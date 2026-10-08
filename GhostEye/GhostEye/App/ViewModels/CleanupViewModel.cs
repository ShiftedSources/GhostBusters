using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Cleanup;

namespace GhostEye.App.ViewModels;

public sealed class CleanupViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly ICleanupScanner _scanner;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? scanCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? cleanCommand;

	public ObservableCollection<CleanupCategoryViewModel> Categories { get; } = new ObservableCollection<CleanupCategoryViewModel>();

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
	public string StatusMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.StatusMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.StatusMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string ResultMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ResultMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ResultMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string EmptyMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.EmptyMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.EmptyMessage);
			}
		}
	}

	public bool HasResults => Categories.Count > 0;

	public long SelectedBytes => Categories.Where((CleanupCategoryViewModel c) => c.IsSelected).Sum((CleanupCategoryViewModel c) => c.Category.Bytes);

	public string SelectedLabel
	{
		get
		{
			if (SelectedBytes != 0L)
			{
				return AppServices.Localizer.Format("cleanup.selected.some", GhostFormat.Bytes(SelectedBytes));
			}
			return AppServices.Localizer["cleanup.selected.none"];
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ScanCommand => scanCommand ?? (scanCommand = new AsyncRelayCommand(ScanAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand CleanCommand => cleanCommand ?? (cleanCommand = new AsyncRelayCommand(CleanAsync));

	public CleanupViewModel()
	{
		_scanner = AppServices.Cleanup;
		StatusMessage = string.Empty;
		ResultMessage = string.Empty;
		EmptyMessage = AppServices.Localizer["cleanup.empty.notRun"];
	}

	public Task ActivatedAsync()
	{
		return Task.CompletedTask;
	}

	public void OnLanguageChanged()
	{
		Categories.Clear();
		StatusMessage = string.Empty;
		ResultMessage = string.Empty;
		EmptyMessage = AppServices.Localizer["cleanup.empty.notRun"];
		RefreshTotals();
	}

	[RelayCommand]
	private async Task ScanAsync()
	{
		ILocalizer loc = AppServices.Localizer;
		IsBusy = true;
		ResultMessage = string.Empty;
		StatusMessage = loc["cleanup.scanning"];
		try
		{
			Progress<string> progress = new Progress<string>((string step) =>
			{
				StatusMessage = loc.Format("cleanup.scanningStep", step);
			});
			CleanupResult cleanupResult = await _scanner.ScanAsync(progress).ConfigureAwait(continueOnCapturedContext: true);
			Categories.Clear();
			foreach (CleanupCategory category in cleanupResult.Categories)
			{
				CleanupCategoryViewModel cleanupCategoryViewModel = new CleanupCategoryViewModel(category);
				cleanupCategoryViewModel.PropertyChanged += (object? _, PropertyChangedEventArgs e) =>
				{
					if (e.PropertyName == "IsSelected")
					{
						RefreshTotals();
					}
				};
				Categories.Add(cleanupCategoryViewModel);
			}
			EmptyMessage = ((cleanupResult.Categories.Count == 0) ? loc["cleanup.empty.nothing"] : string.Empty);
			StatusMessage = ((cleanupResult.Categories.Count == 0) ? string.Empty : loc.Format("cleanup.found", GhostFormat.Bytes(cleanupResult.TotalBytes), cleanupResult.Categories.Count));
			RefreshTotals();
		}
		finally
		{
			IsBusy = false;
		}
	}

	[RelayCommand]
	private async Task CleanAsync()
	{
		if (!(await AdminGate.RequireAdminAsync().ConfigureAwait(continueOnCapturedContext: true)))
		{
			return;
		}
		ILocalizer loc = AppServices.Localizer;
		List<CleanupCategory> selected = (from c in Categories
			where c.IsSelected
			select c.Category).ToList();
		if (selected.Count == 0)
		{
			ResultMessage = loc["cleanup.selected.none"];
		}
		else
		{
			if (!(await AppServices.Dialogs.ConfirmAsync(loc["cleanup.confirm.title"], loc.Format("cleanup.confirm.body", GhostFormat.Bytes(SelectedBytes)), loc["common.delete"], null, destructive: true)))
			{
				return;
			}
			IsBusy = true;
			StatusMessage = loc["cleanup.scanning"];
			try
			{
				Progress<string> progress = new Progress<string>((string step) =>
				{
					StatusMessage = loc.Format("cleanup.cleaningStep", step);
				});
				CleanupOutcome cleanupOutcome = await _scanner.CleanAsync(selected, progress).ConfigureAwait(continueOnCapturedContext: true);
				ResultMessage = ((cleanupOutcome.SkippedFiles == 0) ? loc.Format("cleanup.result", GhostFormat.Bytes(cleanupOutcome.FreedBytes), cleanupOutcome.DeletedFiles) : loc.Format("cleanup.resultSkipped", GhostFormat.Bytes(cleanupOutcome.FreedBytes), cleanupOutcome.DeletedFiles, cleanupOutcome.SkippedFiles));
				StatusMessage = string.Empty;
				await ScanAsync().ConfigureAwait(continueOnCapturedContext: true);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}

	private void RefreshTotals()
	{
		OnPropertyChanged("HasResults");
		OnPropertyChanged("SelectedBytes");
		OnPropertyChanged("SelectedLabel");
	}
}
