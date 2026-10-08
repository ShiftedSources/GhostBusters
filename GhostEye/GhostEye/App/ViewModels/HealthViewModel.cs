using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Health;

namespace GhostEye.App.ViewModels;

public sealed class HealthViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly IHealthService _health;

	private readonly DispatcherTimer _timer;

	private SystemMonitor? _monitor;

	private bool _sampling;

	private bool _loaded;

	private CancellationTokenSource? _repairCancel;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? restartAsAdminCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? refreshCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<DriverViewModel?>? openDriverDownloadCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? openWindowsUpdateCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? openSecuritySettingsCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? runSfcCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? runDismCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? cancelRepairCommand;

	public GaugeViewModel Cpu { get; } = new GaugeViewModel("health.monitor.cpu");

	public GaugeViewModel Memory { get; } = new GaugeViewModel("health.monitor.ram");

	public GaugeViewModel Gpu { get; } = new GaugeViewModel("health.monitor.gpu");

	public GaugeViewModel DiskActivity { get; } = new GaugeViewModel("health.monitor.disk");

	public GaugeViewModel Network { get; } = new GaugeViewModel("health.monitor.network");

	public ObservableCollection<DiskHealthViewModel> Disks { get; } = new ObservableCollection<DiskHealthViewModel>();

	public ObservableCollection<SecurityCheckViewModel> Security { get; } = new ObservableCollection<SecurityCheckViewModel>();

	public ObservableCollection<DriverViewModel> Drivers { get; } = new ObservableCollection<DriverViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsLoading
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsLoading);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsLoading);
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
	public string SecuritySummary
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SecuritySummary);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SecuritySummary);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool ShowOnlyOldDrivers
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ShowOnlyOldDrivers);
				field = value;
				OnShowOnlyOldDriversChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ShowOnlyOldDrivers);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsRepairing
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsRepairing);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsRepairing);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public double RepairPercent
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<double>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RepairPercent);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RepairPercent);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string RepairStatus
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RepairStatus);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RepairStatus);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string RepairLog
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.RepairLog);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.RepairLog);
			}
		}
	}

	public int OldDriverCount => Drivers.Count((DriverViewModel d) => d.IsOld);

	public string DriverSummary => AppServices.Localizer.Format("health.driver.summary", Drivers.Count, OldDriverCount);

	public IEnumerable<DriverViewModel> VisibleDrivers
	{
		get
		{
			if (!ShowOnlyOldDrivers)
			{
				return Drivers;
			}
			return Drivers.Where((DriverViewModel d) => d.IsOld);
		}
	}

	public bool ShowAdminHint => !AdminGate.IsElevated;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand RestartAsAdminCommand => restartAsAdminCommand ?? (restartAsAdminCommand = new RelayCommand(RestartAsAdmin));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RefreshCommand => refreshCommand ?? (refreshCommand = new AsyncRelayCommand(RefreshAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DriverViewModel?> OpenDriverDownloadCommand => openDriverDownloadCommand ?? (openDriverDownloadCommand = new RelayCommand<DriverViewModel>(OpenDriverDownload));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand OpenWindowsUpdateCommand => openWindowsUpdateCommand ?? (openWindowsUpdateCommand = new RelayCommand(OpenWindowsUpdate));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand OpenSecuritySettingsCommand => openSecuritySettingsCommand ?? (openSecuritySettingsCommand = new RelayCommand(OpenSecuritySettings));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RunSfcCommand => runSfcCommand ?? (runSfcCommand = new AsyncRelayCommand(RunSfcAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RunDismCommand => runDismCommand ?? (runDismCommand = new AsyncRelayCommand(RunDismAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand CancelRepairCommand => cancelRepairCommand ?? (cancelRepairCommand = new RelayCommand(CancelRepair));

	public HealthViewModel()
	{
		_health = AppServices.Health;
		StatusMessage = string.Empty;
		RepairLog = string.Empty;
		RepairStatus = string.Empty;
		SecuritySummary = string.Empty;
		_timer = new DispatcherTimer
		{
			Interval = TimeSpan.FromSeconds(1.5)
		};
		_timer.Tick += async (object? _, EventArgs _) =>
		{
			await SampleAsync();
		};
	}

	[RelayCommand]
	private static void RestartAsAdmin()
	{
		AdminGate.RestartElevated("health");
	}

	public async Task ActivatedAsync()
	{
		if (_monitor == null)
		{
			_monitor = await Task.Run(() => new SystemMonitor());
		}
		_timer.Start();
		if (!_loaded)
		{
			_loaded = true;
			await RefreshAsync();
		}
	}

	public void OnLanguageChanged()
	{
		GaugeViewModel[] array = new GaugeViewModel[5] { Cpu, Memory, Gpu, DiskActivity, Network };
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnLanguageChanged();
		}
		RefreshAsync();
	}

	private async Task SampleAsync()
	{
		if (_sampling || _monitor == null)
		{
			return;
		}
		if (AppServices.Shell?.CurrentPage != this)
		{
			_timer.Stop();
			return;
		}
		_sampling = true;
		try
		{
			await ApplySampleAsync();
		}
		catch (Exception exception)
		{
			AppServices.Logger.Error("Monitor sample failed", exception);
		}
		finally
		{
			_sampling = false;
		}
	}

	private async Task ApplySampleAsync()
	{
		MonitorSample monitorSample = await Task.Run((Func<MonitorSample>)_monitor.Sample);
		ILocalizer localizer = AppServices.Localizer;
		Cpu.Percent = monitorSample.CpuPercent;
		Cpu.Value = $"{monitorSample.CpuPercent:0}%";
		GaugeViewModel cpu = Cpu;
		double? cpuTemperatureC = monitorSample.CpuTemperatureC;
		string detail;
		if (cpuTemperatureC.HasValue)
		{
			double valueOrDefault = cpuTemperatureC.GetValueOrDefault();
			detail = localizer.Format("health.monitor.temperature", valueOrDefault);
		}
		else
		{
			detail = localizer["health.monitor.noTemperature"];
		}
		cpu.Detail = detail;
		Memory.Percent = monitorSample.MemoryPercent;
		Memory.Value = $"{monitorSample.MemoryPercent:0}%";
		Memory.Detail = GhostFormat.Bytes(monitorSample.MemoryUsedBytes) + " / " + GhostFormat.Bytes(monitorSample.MemoryTotalBytes);
		Gpu.IsAvailable = monitorSample.GpuPercent.HasValue;
		Gpu.Percent = monitorSample.GpuPercent.GetValueOrDefault();
		GaugeViewModel gpu = Gpu;
		cpuTemperatureC = monitorSample.GpuPercent;
		string value;
		if (cpuTemperatureC.HasValue)
		{
			double valueOrDefault2 = cpuTemperatureC.GetValueOrDefault();
			value = $"{valueOrDefault2:0}%";
		}
		else
		{
			value = "—";
		}
		gpu.Value = value;
		GaugeViewModel gpu2 = Gpu;
		string[] array = new string[2];
		cpuTemperatureC = monitorSample.GpuTemperatureC;
		object obj;
		if (cpuTemperatureC.HasValue)
		{
			double valueOrDefault3 = cpuTemperatureC.GetValueOrDefault();
			obj = localizer.Format("health.monitor.temperature", valueOrDefault3);
		}
		else
		{
			obj = null;
		}
		array[0] = (string)obj;
		array[1] = monitorSample.GpuName?.Replace("NVIDIA GeForce ", string.Empty, StringComparison.OrdinalIgnoreCase);
		gpu2.Detail = string.Join(" · ", array.Where((string x) => x != null));
		DiskActivity.Percent = monitorSample.DiskPercent;
		DiskActivity.Value = $"{monitorSample.DiskPercent:0}%";
		DiskActivity.Detail = localizer["health.monitor.diskDetail"];
		double num = monitorSample.NetworkBitsPerSecond / 1000000.0;
		Network.Percent = Math.Min(100.0, num);
		Network.Value = ((num >= 1.0) ? $"{num:0.0} Mbps" : $"{monitorSample.NetworkBitsPerSecond / 1000.0:0} kbps");
		Network.Detail = localizer["health.monitor.networkDetail"];
	}

	[RelayCommand]
	private async Task RefreshAsync()
	{
		IsLoading = true;
		StatusMessage = AppServices.Localizer["health.loading"];
		try
		{
			Task<IReadOnlyList<DiskHealth>> disks = _health.GetDisksAsync();
			Task<IReadOnlyList<SecurityCheck>> security = _health.GetSecurityAsync();
			Task<IReadOnlyList<DriverInfo>> drivers = _health.GetDriversAsync();
			await Task.WhenAll(disks, security, drivers);
			Disks.Clear();
			foreach (DiskHealth item in disks.Result)
			{
				Disks.Add(new DiskHealthViewModel(item));
			}
			Security.Clear();
			foreach (SecurityCheck item2 in security.Result)
			{
				Security.Add(new SecurityCheckViewModel(item2));
			}
			int num = ((security.Result.Count != 0) ? ((int)Math.Round(100.0 * security.Result.Sum((SecurityCheck c) => c.Level switch
			{
				HealthLevel.Good => 1.0, 
				HealthLevel.Warning => 0.5, 
				HealthLevel.Unknown => 0.75, 
				_ => 0.0, 
			}) / (double)security.Result.Count)) : 0);
			SecuritySummary = AppServices.Localizer.Format("health.security.score", num);
			Drivers.Clear();
			foreach (DriverInfo item3 in drivers.Result)
			{
				Drivers.Add(new DriverViewModel(item3));
			}
			StatusMessage = string.Empty;
		}
		catch (Exception ex)
		{
			AppServices.Logger.Error("Health refresh failed", ex);
			StatusMessage = ex.Message;
		}
		finally
		{
			IsLoading = false;
			OnPropertyChanged("OldDriverCount");
			OnPropertyChanged("DriverSummary");
			OnPropertyChanged("VisibleDrivers");
			OnPropertyChanged("ShowAdminHint");
		}
	}

	[RelayCommand]
	private void OpenDriverDownload(DriverViewModel? driver)
	{
		OpenUrl(driver?.Driver.DownloadUrl ?? "ms-settings:windowsupdate-optionalupdates");
	}

	[RelayCommand]
	private void OpenWindowsUpdate()
	{
		OpenUrl("ms-settings:windowsupdate-optionalupdates");
	}

	[RelayCommand]
	private void OpenSecuritySettings()
	{
		OpenUrl("windowsdefender:");
	}

	private void OpenUrl(string target)
	{
		try
		{
			Process.Start(new ProcessStartInfo(target)
			{
				UseShellExecute = true
			});
		}
		catch (Win32Exception exception)
		{
			AppServices.Logger.Error("Could not open " + target, exception);
		}
	}

	[RelayCommand]
	private Task RunSfcAsync()
	{
		return RepairAsync(RepairTool.SystemFileChecker);
	}

	[RelayCommand]
	private Task RunDismAsync()
	{
		return RepairAsync(RepairTool.Dism);
	}

	[RelayCommand]
	private void CancelRepair()
	{
		_repairCancel?.Cancel();
	}

	private async Task RepairAsync(RepairTool tool)
	{
		if (IsRepairing || !(await AdminGate.RequireAdminAsync("health")))
		{
			return;
		}
		ILocalizer loc = AppServices.Localizer;
		string key = ((tool == RepairTool.SystemFileChecker) ? "sfc" : "dism");
		if (!(await AppServices.Dialogs.ConfirmAsync(loc["health.repair." + key + ".title"], loc["health.repair." + key + ".confirm"], loc["health.repair.start"])))
		{
			return;
		}
		IsRepairing = true;
		RepairPercent = 0.0;
		RepairLog = string.Empty;
		RepairStatus = loc["health.repair." + key + ".running"];
		_repairCancel = new CancellationTokenSource();
		StringBuilder log = new StringBuilder();
		Progress<double> percent = new Progress<double>((double p) =>
		{
			RepairPercent = p;
		});
		Progress<string> lines = new Progress<string>((string line) =>
		{
			log.AppendLine(line);
			RepairLog = log.ToString();
		});
		try
		{
			RepairResult repairResult = await _health.RepairAsync(tool, percent, lines, _repairCancel.Token);
			RepairPercent = 100.0;
			RepairStatus = (repairResult.Success ? loc["health.repair.done"] : loc["health.repair.failed"]) + (string.IsNullOrWhiteSpace(repairResult.Summary) ? string.Empty : (Environment.NewLine + repairResult.Summary));
			AppServices.Logger.Info($"Repair {tool}: success = {repairResult.Success}");
		}
		catch (OperationCanceledException)
		{
			RepairStatus = loc["health.repair.cancelled"];
		}
		catch (Exception ex2)
		{
			AppServices.Logger.Error($"Repair {tool} failed", ex2);
			RepairStatus = ex2.Message;
		}
		finally
		{
			IsRepairing = false;
			_repairCancel.Dispose();
			_repairCancel = null;
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnShowOnlyOldDriversChanged(bool value)
	{
		OnPropertyChanged("VisibleDrivers");
	}
}
