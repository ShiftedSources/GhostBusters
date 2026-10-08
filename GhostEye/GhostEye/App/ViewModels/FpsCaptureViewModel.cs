using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Games;

namespace GhostEye.App.ViewModels;

public sealed class FpsCaptureViewModel : ObservableObject
{
	private static readonly HashSet<string> NotTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "explorer", "GhostEye", "ApplicationFrameHost", "TextInputHost", "SystemSettings", "ShellExperienceHost", "SearchHost" };

	private DispatcherTimer? _countdown;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<object?>? setDurationCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? refreshTargetsCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<FpsTargetViewModel?>? selectTargetCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? captureCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<FpsEntryViewModel?>? deleteEntryCommand;

	public ObservableCollection<FpsTargetViewModel> Targets { get; } = new ObservableCollection<FpsTargetViewModel>();

	public ObservableCollection<FpsEntryViewModel> History { get; } = new ObservableCollection<FpsEntryViewModel>();

	public IReadOnlyList<int> Durations { get; } = new _003C_003Ez__ReadOnlyArray<int>(new int[3] { 30, 60, 120 });

	[ObservableProperty]
	[NotifyPropertyChangedFor("Is30", new string[] { "Is60", "Is120" })]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public int Duration
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<int>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Duration);
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Is30);
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Is60);
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Is120);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Duration);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Is30);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Is60);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Is120);
			}
		}
	}

	public bool Is30 => Duration == 30;

	public bool Is60 => Duration == 60;

	public bool Is120 => Duration == 120;

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

	public bool HasTargets => Targets.Count > 0;

	public bool HasHistory => History.Count > 0;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<object?> SetDurationCommand => setDurationCommand ?? (setDurationCommand = new RelayCommand<object>(SetDuration));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand RefreshTargetsCommand => refreshTargetsCommand ?? (refreshTargetsCommand = new RelayCommand(RefreshTargets));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<FpsTargetViewModel?> SelectTargetCommand => selectTargetCommand ?? (selectTargetCommand = new RelayCommand<FpsTargetViewModel>(SelectTarget));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand CaptureCommand => captureCommand ?? (captureCommand = new AsyncRelayCommand(CaptureAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<FpsEntryViewModel?> DeleteEntryCommand => deleteEntryCommand ?? (deleteEntryCommand = new RelayCommand<FpsEntryViewModel>(DeleteEntry));

	public FpsCaptureViewModel()
	{
		StatusMessage = string.Empty;
		Duration = 30;
		LoadHistory();
	}

	[RelayCommand]
	private void SetDuration(object? seconds)
	{
		if (seconds != null && int.TryParse(seconds.ToString(), out var result))
		{
			Duration = result;
		}
		OnPropertyChanged("Is30");
		OnPropertyChanged("Is60");
		OnPropertyChanged("Is120");
	}

	[RelayCommand]
	public void RefreshTargets()
	{
		int? previous = Targets.FirstOrDefault((FpsTargetViewModel t) => t.IsSelected)?.ProcessId;
		Targets.Clear();
		IReadOnlyList<GameEntry> library = BackgroundAgent.GetLibrary();
		HashSet<int> hashSet = new HashSet<int>();
		Process[] processes = Process.GetProcesses();
		foreach (Process process in processes)
		{
			using (process)
			{
				try
				{
					if (process.MainWindowHandle != IntPtr.Zero && !string.IsNullOrWhiteSpace(process.MainWindowTitle) && !NotTargets.Contains(process.ProcessName) && hashSet.Add(process.Id))
					{
						string path = RunningGames.GetPath(process.Id);
						GameEntry gameEntry = ((path == null) ? null : library.FirstOrDefault((GameEntry g) => g.Owns(path)));
						Targets.Add(new FpsTargetViewModel(gameEntry?.Name ?? process.ProcessName, process.Id, (object)gameEntry != null));
					}
				}
				catch (InvalidOperationException)
				{
				}
			}
		}
		List<FpsTargetViewModel> list = (from t in Targets
			orderby t.IsGame descending, t.Name
			select t).ToList();
		Targets.Clear();
		foreach (FpsTargetViewModel item in list)
		{
			Targets.Add(item);
		}
		FpsTargetViewModel fpsTargetViewModel = Targets.FirstOrDefault((FpsTargetViewModel t) => t.ProcessId == previous) ?? Targets.FirstOrDefault((FpsTargetViewModel t) => t.IsGame);
		if (fpsTargetViewModel != null)
		{
			fpsTargetViewModel.IsSelected = true;
		}
		OnPropertyChanged("HasTargets");
	}

	[RelayCommand]
	private void SelectTarget(FpsTargetViewModel? target)
	{
		foreach (FpsTargetViewModel target2 in Targets)
		{
			target2.IsSelected = target2 == target;
			target2.RefreshSelection();
		}
	}

	[RelayCommand]
	private async Task CaptureAsync()
	{
		FpsTargetViewModel target = Targets.FirstOrDefault((FpsTargetViewModel t) => t.IsSelected);
		ILocalizer loc = AppServices.Localizer;
		if (target == null)
		{
			StatusMessage = loc["fps.error.noTarget"];
			return;
		}
		bool flag = !(await AdminGate.RequireAdminAsync("gaming"));
		if (flag)
		{
			return;
		}
		if (FrameCaptureTool.Ensure() == null)
		{
			StatusMessage = loc["fps.error.missingTool"];
			return;
		}
		IsBusy = true;
		int remaining = Duration;
		StatusMessage = loc.Format("fps.capturing", target.Name, remaining);
		_countdown = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1.0)
		};
		_countdown.Tick += (object? _, EventArgs _) =>
		{
			remaining = Math.Max(0, remaining - 1);
			StatusMessage = ((remaining > 0) ? loc.Format("fps.capturing", target.Name, remaining) : loc["fps.analysing"]);
		};
		_countdown.Start();
		try
		{
			FrameCaptureResult result = await AppServices.FrameCapture.CaptureAsync(target.ProcessId, Duration);
			if (!result.Success || (object)result.Stats == null)
			{
				StatusMessage = result.Error;
				return;
			}
			int count = (await AppServices.Drift.GetActiveTweakIdsAsync()).Count;
			AppServices.FrameCaptures.Add(new FrameCaptureEntry
			{
				Id = Guid.NewGuid().ToString("N"),
				GameName = target.Name,
				CapturedAt = DateTimeOffset.Now,
				Stats = result.Stats,
				ActiveTweaks = count
			});
			StatusMessage = loc.Format("fps.done", Math.Round(result.Stats.AverageFps), Math.Round(result.Stats.OnePercentLowFps), result.Stats.AverageFrameTimeMs.ToString("0.0", loc.Culture));
			AppServices.Logger.Info($"FPS capture of {target.Name}: {result.Stats.AverageFps:0.0} avg, {result.Stats.OnePercentLowFps:0.0} 1% low, {result.Stats.Frames} frames.");
			LoadHistory();
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("FPS capture failed", ex);
			StatusMessage = ex.Message;
		}
		finally
		{
			_countdown?.Stop();
			IsBusy = false;
		}
	}

	[RelayCommand]
	private void DeleteEntry(FpsEntryViewModel? entry)
	{
		if (entry != null)
		{
			AppServices.FrameCaptures.Delete(entry.Entry.Id);
			LoadHistory();
		}
	}

	private void LoadHistory()
	{
		IReadOnlyList<FrameCaptureEntry> all = AppServices.FrameCaptures.GetAll();
		History.Clear();
		foreach (FrameCaptureEntry entry in all.Take(12))
		{
			FrameCaptureEntry previous = (from e in all
				where e.Id != entry.Id && e.GameName == entry.GameName && e.CapturedAt < entry.CapturedAt
				orderby e.CapturedAt descending
				select e).FirstOrDefault();
			History.Add(new FpsEntryViewModel(entry, previous));
		}
		OnPropertyChanged("HasHistory");
	}

	public void OnLanguageChanged()
	{
		StatusMessage = string.Empty;
		LoadHistory();
	}
}
