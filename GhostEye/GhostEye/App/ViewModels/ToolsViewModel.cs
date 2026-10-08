using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class ToolsViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private const long LargeFileThreshold = 524288000L;

	private readonly IPerformanceToolsService _tools;

	private readonly DispatcherTimer _memoryTimer;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? freeMemoryCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshProcessesCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<ProcessItemViewModel?>? endProcessCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? flushDnsCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? findLargeFilesCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<LargeFileViewModel?>? openFileLocationCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand<LargeFileViewModel?>? deleteFileCommand;

	public ObservableCollection<ProcessItemViewModel> Processes { get; } = new ObservableCollection<ProcessItemViewModel>();

	public ObservableCollection<LargeFileViewModel> LargeFiles { get; } = new ObservableCollection<LargeFileViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string MemoryLabel
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.MemoryLabel);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.MemoryLabel);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public double MemoryPercent
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<double>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.MemoryPercent);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.MemoryPercent);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsRamBusy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsRamBusy);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsRamBusy);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string RamMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RamMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RamMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsProcessBusy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsProcessBusy);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsProcessBusy);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string ProcessMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ProcessMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ProcessMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string DnsMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DnsMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DnsMessage);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsFilesBusy
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsFilesBusy);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsFilesBusy);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string FilesMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.FilesMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.FilesMessage);
			}
		}
	}

	public bool HasLargeFiles => LargeFiles.Count > 0;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand FreeMemoryCommand => freeMemoryCommand ?? (freeMemoryCommand = new AsyncRelayCommand(FreeMemoryAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshProcessesCommand => refreshProcessesCommand ?? (refreshProcessesCommand = new AsyncRelayCommand(RefreshProcessesAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<ProcessItemViewModel?> EndProcessCommand => endProcessCommand ?? (endProcessCommand = new AsyncRelayCommand<ProcessItemViewModel>(EndProcessAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand FlushDnsCommand => flushDnsCommand ?? (flushDnsCommand = new AsyncRelayCommand(FlushDnsAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand FindLargeFilesCommand => findLargeFilesCommand ?? (findLargeFilesCommand = new AsyncRelayCommand(FindLargeFilesAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<LargeFileViewModel?> OpenFileLocationCommand => openFileLocationCommand ?? (openFileLocationCommand = new RelayCommand<LargeFileViewModel>(OpenFileLocation));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand<LargeFileViewModel?> DeleteFileCommand => deleteFileCommand ?? (deleteFileCommand = new AsyncRelayCommand<LargeFileViewModel>(DeleteFileAsync));

	public ToolsViewModel()
	{
		_tools = AppServices.PerformanceTools;
		RamMessage = string.Empty;
		DnsMessage = string.Empty;
		ProcessMessage = string.Empty;
		FilesMessage = string.Empty;
		MemoryLabel = string.Empty;
		_memoryTimer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(2.0)
		};
		_memoryTimer.Tick += (object? _, EventArgs _) =>
		{
			if (AppServices.Shell?.CurrentPage != this)
			{
				_memoryTimer.Stop();
			}
			else
			{
				UpdateMemory();
			}
		};
		UpdateMemory();
	}

	public async Task ActivatedAsync()
	{
		_memoryTimer.Start();
		UpdateMemory();
		if (Processes.Count == 0)
		{
			await RefreshProcessesAsync().ConfigureAwait(continueOnCapturedContext: true);
		}
	}

	public void OnLanguageChanged()
	{
		UpdateMemory();
		RamMessage = string.Empty;
		DnsMessage = string.Empty;
		FilesMessage = string.Empty;
	}

	private void UpdateMemory()
	{
		MemoryStatus memoryStatus = _tools.GetMemoryStatus();
		MemoryPercent = Math.Round(memoryStatus.UsedPercent);
		MemoryLabel = AppServices.Localizer.Format("tools.ram.status", GhostFormat.Bytes(memoryStatus.UsedBytes), GhostFormat.Bytes(memoryStatus.TotalBytes), MemoryPercent);
	}

	[RelayCommand]
	private async Task FreeMemoryAsync()
	{
		IsRamBusy = true;
		RamMessage = AppServices.Localizer["tools.ram.working"];
		try
		{
			MemoryCleanResult memoryCleanResult = await _tools.FreeMemoryAsync().ConfigureAwait(continueOnCapturedContext: true);
			RamMessage = AppServices.Localizer.Format("tools.ram.freed", GhostFormat.Bytes(memoryCleanResult.FreedBytes)) + " " + memoryCleanResult.Message;
		}
		finally
		{
			IsRamBusy = false;
			UpdateMemory();
		}
	}

	[RelayCommand]
	private async Task RefreshProcessesAsync()
	{
		IsProcessBusy = true;
		try
		{
			IReadOnlyList<ProcessUsage> readOnlyList = await _tools.GetHeavyProcessesAsync(10).ConfigureAwait(continueOnCapturedContext: true);
			Processes.Clear();
			foreach (ProcessUsage item in readOnlyList)
			{
				Processes.Add(new ProcessItemViewModel(item));
			}
			ProcessMessage = string.Empty;
		}
		catch (Exception ex) when ((ex is InvalidOperationException || ex is Win32Exception) ? true : false)
		{
			AppServices.Logger.Error("Process list failed", ex);
			ProcessMessage = ex.Message;
		}
		finally
		{
			IsProcessBusy = false;
		}
	}

	[RelayCommand]
	private async Task EndProcessAsync(ProcessItemViewModel? item)
	{
		if (item != null)
		{
			ILocalizer loc = AppServices.Localizer;
			if (await AppServices.Dialogs.ConfirmAsync(loc["tools.processes.confirm.title"], loc.Format("tools.processes.confirm.body", item.Name), loc["common.endTask"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true))
			{
				OperationResult operationResult = _tools.EndProcess(item.Process.Id);
				ProcessMessage = (operationResult.Success ? loc.Format("tools.processes.ended", item.Name) : operationResult.Error);
				await RefreshProcessesAsync().ConfigureAwait(continueOnCapturedContext: true);
			}
		}
	}

	[RelayCommand]
	private async Task FlushDnsAsync()
	{
		OperationResult operationResult = await _tools.FlushDnsAsync().ConfigureAwait(continueOnCapturedContext: true);
		DnsMessage = (operationResult.Success ? AppServices.Localizer["tools.dns.done"] : operationResult.Error);
	}

	[RelayCommand]
	private async Task FindLargeFilesAsync()
	{
		IsFilesBusy = true;
		FilesMessage = AppServices.Localizer["tools.files.scanning"];
		try
		{
			Progress<string> progress = new Progress<string>((string folder) =>
			{
				FilesMessage = AppServices.Localizer.Format("tools.files.scanningIn", folder);
			});
			IReadOnlyList<LargeFile> readOnlyList = await _tools.FindLargeFilesAsync(524288000L, 50, progress).ConfigureAwait(continueOnCapturedContext: true);
			LargeFiles.Clear();
			foreach (LargeFile item in readOnlyList)
			{
				LargeFiles.Add(new LargeFileViewModel(item));
			}
			FilesMessage = ((readOnlyList.Count == 0) ? AppServices.Localizer["tools.files.none"] : AppServices.Localizer.Format("tools.files.found", readOnlyList.Count, GhostFormat.Bytes(readOnlyList.Sum((LargeFile f) => f.Bytes))));
		}
		finally
		{
			IsFilesBusy = false;
			OnPropertyChanged("HasLargeFiles");
		}
	}

	[RelayCommand]
	private void OpenFileLocation(LargeFileViewModel? file)
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
	private async Task DeleteFileAsync(LargeFileViewModel? file)
	{
		if (file == null)
		{
			return;
		}
		ILocalizer loc = AppServices.Localizer;
		if (!(await AppServices.Dialogs.ConfirmAsync(loc["tools.files.confirm.title"], loc.Format("tools.files.confirm.body", file.Name, file.SizeLabel), loc["common.delete"], null, destructive: true).ConfigureAwait(continueOnCapturedContext: true)))
		{
			return;
		}
		try
		{
			ShellFileOperations.SendToRecycleBin(file.File.Path);
			LargeFiles.Remove(file);
			FilesMessage = loc.Format("tools.files.deleted", file.Name);
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException || ex is OperationCanceledException) ? true : false)
		{
			FilesMessage = ex.Message;
		}
		finally
		{
			OnPropertyChanged("HasLargeFiles");
		}
	}
}
