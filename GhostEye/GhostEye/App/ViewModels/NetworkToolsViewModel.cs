using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class NetworkToolsViewModel : ObservableObject
{
	private readonly INetworkToolsService _network;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? speedTestCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? pingAllCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? resetNetworkCommand;

	public ObservableCollection<PingRowViewModel> Pings { get; } = new ObservableCollection<PingRowViewModel>();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Download
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Download);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Download);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Upload
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Upload);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Upload);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string Latency
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.Latency);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.Latency);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string SpeedStatus
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SpeedStatus);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SpeedStatus);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsTesting
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsTesting);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsTesting);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsPinging
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsPinging);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsPinging);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsResetting
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsResetting);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsResetting);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string ResetStatus
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ResetStatus);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ResetStatus);
			}
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand SpeedTestCommand => speedTestCommand ?? (speedTestCommand = new AsyncRelayCommand(SpeedTestAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand PingAllCommand => pingAllCommand ?? (pingAllCommand = new AsyncRelayCommand(PingAllAsync));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand ResetNetworkCommand => resetNetworkCommand ?? (resetNetworkCommand = new AsyncRelayCommand(ResetNetworkAsync));

	public NetworkToolsViewModel()
	{
		_network = AppServices.NetworkTools;
		Download = "—";
		Upload = "—";
		Latency = "—";
		SpeedStatus = string.Empty;
		ResetStatus = string.Empty;
		foreach (PingTarget pingTarget in _network.PingTargets)
		{
			Pings.Add(new PingRowViewModel(pingTarget));
		}
	}

	public void OnLanguageChanged()
	{
		foreach (PingRowViewModel ping in Pings)
		{
			ping.OnLanguageChanged();
		}
	}

	[RelayCommand]
	private async Task SpeedTestAsync()
	{
		IsTesting = true;
		NetworkToolsViewModel networkToolsViewModel = this;
		NetworkToolsViewModel networkToolsViewModel2 = this;
		string text = (Latency = "…");
		string download = (networkToolsViewModel2.Upload = text);
		networkToolsViewModel.Download = download;
		Progress<string> progress = new Progress<string>((string speedStatus) =>
		{
			SpeedStatus = speedStatus;
		});
		try
		{
			SpeedResult speedResult = await _network.SpeedTestAsync(progress);
			NetworkToolsViewModel networkToolsViewModel3 = this;
			double? downloadMbps = speedResult.DownloadMbps;
			string download2;
			if (downloadMbps.HasValue)
			{
				double valueOrDefault = downloadMbps.GetValueOrDefault();
				download2 = $"{valueOrDefault:0.0}";
			}
			else
			{
				download2 = "—";
			}
			networkToolsViewModel3.Download = download2;
			NetworkToolsViewModel networkToolsViewModel4 = this;
			downloadMbps = speedResult.UploadMbps;
			string upload;
			if (downloadMbps.HasValue)
			{
				double valueOrDefault2 = downloadMbps.GetValueOrDefault();
				upload = $"{valueOrDefault2:0.0}";
			}
			else
			{
				upload = "—";
			}
			networkToolsViewModel4.Upload = upload;
			NetworkToolsViewModel networkToolsViewModel5 = this;
			downloadMbps = speedResult.LatencyMs;
			string latency;
			if (downloadMbps.HasValue)
			{
				double valueOrDefault3 = downloadMbps.GetValueOrDefault();
				latency = $"{valueOrDefault3:0}";
			}
			else
			{
				latency = "—";
			}
			networkToolsViewModel5.Latency = latency;
			SpeedStatus = speedResult.Error ?? AppServices.Localizer.Format("network.speed.done", DateTime.Now.ToString("t", AppServices.Localizer.Culture));
		}
		finally
		{
			IsTesting = false;
		}
	}

	[RelayCommand]
	private async Task PingAllAsync()
	{
		IsPinging = true;
		try
		{
			foreach (PingRowViewModel ping in Pings)
			{
				ping.Value = "…";
				ping.HasResult = false;
			}
			await Task.WhenAll(((IEnumerable<PingRowViewModel>)Pings).Select((Func<PingRowViewModel, Task>)(async (PingRowViewModel row) =>
			{
				PingResult pingResult = await _network.PingAsync(row.Target);
				ILocalizer localizer = AppServices.Localizer;
				row.HasResult = true;
				double? averageMs = pingResult.AverageMs;
				if (averageMs.HasValue)
				{
					double valueOrDefault = averageMs.GetValueOrDefault();
					row.Value = $"{valueOrDefault:0} ms";
					row.Detail = localizer.Format("network.ping.detail", pingResult.JitterMs.GetValueOrDefault(), pingResult.LossPercent);
					row.Quality = ((pingResult.LossPercent > 0 || valueOrDefault >= 100.0) ? RiskLevel.High : ((valueOrDefault >= 50.0) ? RiskLevel.Medium : RiskLevel.Low));
				}
				else
				{
					row.Value = localizer["network.ping.noReply"];
					row.Detail = localizer["network.ping.noReplyDetail"];
					row.Quality = RiskLevel.High;
				}
			})));
		}
		finally
		{
			IsPinging = false;
		}
	}

	[RelayCommand]
	private async Task ResetNetworkAsync()
	{
		if (!(await AdminGate.RequireAdminAsync("network")))
		{
			return;
		}
		ILocalizer loc = AppServices.Localizer;
		if (!(await AppServices.Dialogs.ConfirmAsync(loc["network.reset.title"], loc["network.reset.body"], loc["network.reset.button"], null, destructive: true)))
		{
			return;
		}
		IsResetting = true;
		try
		{
			Progress<string> progress = new Progress<string>((string step) =>
			{
				ResetStatus = step;
			});
			OperationResult operationResult = await _network.ResetNetworkAsync(progress);
			ResetStatus = (operationResult.Success ? loc["network.reset.done"] : operationResult.Error);
			AppServices.Logger.Info($"Network reset: success = {operationResult.Success}");
			if (operationResult.Success)
			{
				await AppServices.Dialogs.AlertAsync(loc["dialog.restart.title"], loc["network.reset.done"]);
			}
		}
		finally
		{
			IsResetting = false;
		}
	}
}
