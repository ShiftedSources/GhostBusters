using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Disk;

namespace GhostEye.App.ViewModels;

public sealed class DiskViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly IDiskAnalyzer _analyzer;

	private CancellationTokenSource? _mapCancel;

	private CancellationTokenSource? _dupCancel;

	private bool _loaded;

	private int _mapGeneration;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<DriveViewModel?>? analyzeDriveCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? analyzeProfileCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<FolderBarViewModel?>? openBarCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? goUpCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? cancelMapCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<FolderBarViewModel?>? showInExplorerCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? findDuplicatesCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? cancelDuplicatesCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<DuplicateFileViewModel?>? openDuplicateCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? deleteDuplicatesCommand;

	public ObservableCollection<DriveViewModel> Drives { get; } = new ObservableCollection<DriveViewModel>();

	public ObservableCollection<FolderBarViewModel> Bars { get; } = new ObservableCollection<FolderBarViewModel>();

	public ObservableCollection<DuplicateGroupViewModel> Duplicates { get; } = new ObservableCollection<DuplicateGroupViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string CurrentFolder
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.CurrentFolder);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.CurrentFolder);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsMapping
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsMapping);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsMapping);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string MapStatus
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.MapStatus);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.MapStatus);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsSearchingDuplicates
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsSearchingDuplicates);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsSearchingDuplicates);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string DupStatus
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DupStatus);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DupStatus);
			}
		}
	}

	public bool CanGoUp
	{
		get
		{
			if (!string.IsNullOrEmpty(CurrentFolder))
			{
				return Directory.GetParent(CurrentFolder) != null;
			}
			return false;
		}
	}

	public bool HasBars => Bars.Count > 0;

	public bool HasDuplicates => Duplicates.Count > 0;

	public long SelectedDuplicateBytes => Duplicates.Sum((DuplicateGroupViewModel g) => g.Files.Count((DuplicateFileViewModel f) => f.IsSelected) * g.Group.SizeBytes);

	public string DuplicateSelectionLabel => AppServices.Localizer.Format("disk.dup.selection", Duplicates.Sum((DuplicateGroupViewModel g) => g.Files.Count((DuplicateFileViewModel f) => f.IsSelected)), GhostFormat.Bytes(SelectedDuplicateBytes));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<DriveViewModel?> AnalyzeDriveCommand => analyzeDriveCommand ?? (analyzeDriveCommand = new AsyncRelayCommand<DriveViewModel>(AnalyzeDriveAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand AnalyzeProfileCommand => analyzeProfileCommand ?? (analyzeProfileCommand = new AsyncRelayCommand(AnalyzeProfileAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<FolderBarViewModel?> OpenBarCommand => openBarCommand ?? (openBarCommand = new AsyncRelayCommand<FolderBarViewModel>(OpenBarAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand GoUpCommand => goUpCommand ?? (goUpCommand = new AsyncRelayCommand(GoUpAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand CancelMapCommand => cancelMapCommand ?? (cancelMapCommand = new RelayCommand(CancelMap));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<FolderBarViewModel?> ShowInExplorerCommand => showInExplorerCommand ?? (showInExplorerCommand = new RelayCommand<FolderBarViewModel>(ShowInExplorer));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand FindDuplicatesCommand => findDuplicatesCommand ?? (findDuplicatesCommand = new AsyncRelayCommand(FindDuplicatesAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand CancelDuplicatesCommand => cancelDuplicatesCommand ?? (cancelDuplicatesCommand = new RelayCommand(CancelDuplicates));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DuplicateFileViewModel?> OpenDuplicateCommand => openDuplicateCommand ?? (openDuplicateCommand = new RelayCommand<DuplicateFileViewModel>(OpenDuplicate));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand DeleteDuplicatesCommand => deleteDuplicatesCommand ?? (deleteDuplicatesCommand = new AsyncRelayCommand(DeleteDuplicatesAsync));

	public DiskViewModel()
	{
		_analyzer = AppServices.DiskAnalyzer;
		CurrentFolder = string.Empty;
		MapStatus = string.Empty;
		DupStatus = string.Empty;
	}

	public Task ActivatedAsync()
	{
		if (!_loaded)
		{
			_loaded = true;
			RefreshDrives();
		}
		return Task.CompletedTask;
	}

	public void OnLanguageChanged()
	{
		RefreshDrives();
		OnPropertyChanged("DuplicateSelectionLabel");
	}

	private void RefreshDrives()
	{
		Drives.Clear();
		foreach (DriveSummary drife in _analyzer.GetDrives())
		{
			Drives.Add(new DriveViewModel(drife));
		}
	}

	[RelayCommand]
	private Task AnalyzeDriveAsync(DriveViewModel? drive)
	{
		if (drive != null)
		{
			return MapAsync(drive.Drive.Root);
		}
		return Task.CompletedTask;
	}

	[RelayCommand]
	private Task AnalyzeProfileAsync()
	{
		return MapAsync(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
	}

	[RelayCommand]
	private Task OpenBarAsync(FolderBarViewModel? bar)
	{
		if (bar == null || !bar.CanOpen)
		{
			return Task.CompletedTask;
		}
		return MapAsync(bar.Usage.Path);
	}

	[RelayCommand]
	private Task GoUpAsync()
	{
		DirectoryInfo parent = Directory.GetParent(CurrentFolder);
		if (parent == null)
		{
			return Task.CompletedTask;
		}
		return MapAsync(parent.FullName);
	}

	[RelayCommand]
	private void CancelMap()
	{
		_mapCancel?.Cancel();
	}

	[RelayCommand]
	private void ShowInExplorer(FolderBarViewModel? bar)
	{
		string text = bar?.Usage.Path ?? CurrentFolder;
		if (Directory.Exists(text))
		{
			Process.Start(new ProcessStartInfo("explorer.exe", "\"" + text + "\"")
			{
				UseShellExecute = true
			});
		}
	}

	private async Task MapAsync(string folder)
	{
		_mapCancel?.Cancel();
		_mapCancel = new CancellationTokenSource();
		CancellationToken token = _mapCancel.Token;
		int generation = ++_mapGeneration;
		CurrentFolder = folder;
		IsMapping = true;
		MapStatus = AppServices.Localizer["disk.map.scanning"];
		Bars.Clear();
		OnPropertyChanged("CanGoUp");
		OnPropertyChanged("HasBars");
		ILocalizer loc = AppServices.Localizer;
		DateTime lastReport = DateTime.MinValue;
		Progress<string> progress = new Progress<string>((string path) =>
		{
			if (IsMapping && generation == _mapGeneration && DateTime.UtcNow - lastReport > TimeSpan.FromMilliseconds(250.0))
			{
				lastReport = DateTime.UtcNow;
				MapStatus = loc.Format("disk.map.scanningIn", path);
			}
		});
		try
		{
			IReadOnlyList<FolderUsage> readOnlyList = await _analyzer.MeasureAsync(folder, progress, token);
			if (generation != _mapGeneration)
			{
				return;
			}
			long largest = ((readOnlyList.Count == 0) ? 0 : readOnlyList.Max((FolderUsage u) => u.Bytes));
			long num = readOnlyList.Sum((FolderUsage u) => u.Bytes);
			foreach (FolderUsage item in readOnlyList)
			{
				Bars.Add(new FolderBarViewModel(item, largest, num));
			}
			MapStatus = loc.Format("disk.map.total", GhostFormat.Bytes(num), folder);
		}
		catch (OperationCanceledException) when (generation == _mapGeneration)
		{
			MapStatus = loc["disk.map.cancelled"];
		}
		catch (OperationCanceledException)
		{
		}
		finally
		{
			if (generation == _mapGeneration)
			{
				IsMapping = false;
				OnPropertyChanged("HasBars");
			}
		}
	}

	[RelayCommand]
	private async Task FindDuplicatesAsync()
	{
		_dupCancel?.Cancel();
		_dupCancel = new CancellationTokenSource();
		CancellationToken token = _dupCancel.Token;
		ILocalizer loc = AppServices.Localizer;
		IsSearchingDuplicates = true;
		DupStatus = loc["disk.dup.scanning"];
		Duplicates.Clear();
		RefreshDuplicateTotals();
		DateTime lastReport = DateTime.MinValue;
		Progress<string> progress = new Progress<string>((string path) =>
		{
			if (IsSearchingDuplicates && DateTime.UtcNow - lastReport > TimeSpan.FromMilliseconds(250.0))
			{
				lastReport = DateTime.UtcNow;
				DupStatus = loc.Format("disk.dup.scanningIn", path);
			}
		});
		try
		{
			string[] roots = new string[1] { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) };
			IReadOnlyList<DuplicateGroup> readOnlyList = await _analyzer.FindDuplicatesAsync(roots, 1048576L, progress, token);
			foreach (DuplicateGroup item in readOnlyList.Take(300))
			{
				DuplicateGroupViewModel duplicateGroupViewModel = new DuplicateGroupViewModel(item);
				foreach (DuplicateFileViewModel file in duplicateGroupViewModel.Files)
				{
					file.PropertyChanged += (object? _, PropertyChangedEventArgs _) =>
					{
						RefreshDuplicateTotals();
					};
				}
				Duplicates.Add(duplicateGroupViewModel);
			}
			DupStatus = ((readOnlyList.Count == 0) ? loc["disk.dup.none"] : loc.Format("disk.dup.found", readOnlyList.Count, GhostFormat.Bytes(readOnlyList.Sum((DuplicateGroup g) => g.WastedBytes))));
		}
		catch (OperationCanceledException)
		{
			DupStatus = loc["disk.map.cancelled"];
		}
		finally
		{
			IsSearchingDuplicates = false;
			RefreshDuplicateTotals();
		}
	}

	[RelayCommand]
	private void CancelDuplicates()
	{
		_dupCancel?.Cancel();
	}

	[RelayCommand]
	private void OpenDuplicate(DuplicateFileViewModel? file)
	{
		if (file != null && File.Exists(file.File.Path))
		{
			Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + file.File.Path + "\"")
			{
				UseShellExecute = true
			});
		}
	}

	[RelayCommand]
	private async Task DeleteDuplicatesAsync()
	{
		ILocalizer loc = AppServices.Localizer;
		List<(DuplicateGroupViewModel Group, DuplicateFileViewModel File)> selected = Duplicates.SelectMany((DuplicateGroupViewModel g) => from f in g.Files
			where f.IsSelected
			select (Group: g, File: f)).ToList();
		if (selected.Count == 0)
		{
			DupStatus = loc["disk.dup.nothingSelected"];
		}
		else if (Duplicates.Any((DuplicateGroupViewModel g) => g.Files.All((DuplicateFileViewModel f) => f.IsSelected)))
		{
			await AppServices.Dialogs.AlertAsync(loc["disk.dup.keepOne.title"], loc["disk.dup.keepOne.body"]);
		}
		else
		{
			if (!(await AppServices.Dialogs.ConfirmAsync(loc["disk.dup.confirm.title"], loc.Format("disk.dup.confirm.body", selected.Count, GhostFormat.Bytes(SelectedDuplicateBytes)), loc["common.delete"], null, destructive: true)))
			{
				return;
			}
			int moved = 0;
			int failed = 0;
			foreach (var item in selected)
			{
				var (group, file) = item;
				try
				{
					await Task.Run(() =>
					{
						ShellFileOperations.SendToRecycleBin(file.File.Path);
					});
					group.Files.Remove(file);
					moved++;
				}
				catch (IOException)
				{
					failed++;
				}
			}
			foreach (DuplicateGroupViewModel item2 in Duplicates.Where((DuplicateGroupViewModel g) => g.Files.Count < 2).ToList())
			{
				Duplicates.Remove(item2);
			}
			DupStatus = ((failed == 0) ? loc.Format("disk.dup.deleted", moved) : loc.Format("disk.dup.deletedPartial", moved, failed));
			RefreshDuplicateTotals();
		}
	}

	private void RefreshDuplicateTotals()
	{
		OnPropertyChanged("HasDuplicates");
		OnPropertyChanged("SelectedDuplicateBytes");
		OnPropertyChanged("DuplicateSelectionLabel");
	}
}
