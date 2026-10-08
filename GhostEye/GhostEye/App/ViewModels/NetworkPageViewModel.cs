using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;
using GhostEye.Infrastructure.Tweaks;

namespace GhostEye.App.ViewModels;

public sealed class NetworkPageViewModel : ObservableObject, IPageViewModel, ILocalizedViewModel
{
	private readonly AppSettingsService _settings;

	private readonly bool _initialized;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<DnsPreset?>? usePresetCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand<DnsBenchmarkRowViewModel?>? useBenchmarkResultCommand;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private AsyncRelayCommand? runBenchmarkCommand;

	public CategoryPageViewModel Tweaks { get; }

	public IReadOnlyList<DnsPreset> Presets { get; } = DnsPresets.All;

	public ObservableCollection<DnsBenchmarkRowViewModel> BenchmarkResults { get; } = new ObservableCollection<DnsBenchmarkRowViewModel>();

	[ObservableProperty]
	[NotifyCanExecuteChangedFor("RunBenchmarkCommand")]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsBenchmarking
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsBenchmarking);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsBenchmarking);
				RunBenchmarkCommand.NotifyCanExecuteChanged();
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string BenchmarkStatus
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.BenchmarkStatus);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.BenchmarkStatus);
			}
		}
	} = string.Empty;

	public NetworkToolsViewModel Tools { get; } = new NetworkToolsViewModel();

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string PrimaryDns
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.PrimaryDns);
				field = value;
				OnPrimaryDnsChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.PrimaryDns);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string SecondaryDns
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.SecondaryDns);
				field = value;
				OnSecondaryDnsChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.SecondaryDns);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public string DnsValidationMessage
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<string>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.DnsValidationMessage);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.DnsValidationMessage);
			}
		}
	}

	public string DnsWarning => AppServices.Localizer["network.dns.warning"];

	public string DnsTitle => AppServices.Localizer["network.dns.title"];

	public string PrimaryLabel => AppServices.Localizer["network.dns.primary"];

	public string SecondaryLabel => AppServices.Localizer["network.dns.secondary"];

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DnsPreset?> UsePresetCommand => usePresetCommand ?? (usePresetCommand = new RelayCommand<DnsPreset>(UsePreset));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand<DnsBenchmarkRowViewModel?> UseBenchmarkResultCommand => useBenchmarkResultCommand ?? (useBenchmarkResultCommand = new RelayCommand<DnsBenchmarkRowViewModel>(UseBenchmarkResult));

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IAsyncRelayCommand RunBenchmarkCommand => runBenchmarkCommand ?? (runBenchmarkCommand = new AsyncRelayCommand(RunBenchmarkAsync, CanRunBenchmark));

	public NetworkPageViewModel()
	{
		_settings = AppServices.Settings;
		DnsValidationMessage = string.Empty;
		PrimaryDns = _settings.Current.PrimaryDns;
		SecondaryDns = _settings.Current.SecondaryDns;
		Tweaks = new CategoryPageViewModel(TweakCategory.Network, "network.category", "Icon.Network", "history.title.network");
		_initialized = true;
	}

	[RelayCommand]
	private void UsePreset(DnsPreset? preset)
	{
		if ((object)preset != null)
		{
			UseServers(preset.Primary, preset.Secondary);
			BenchmarkStatus = AppServices.Localizer.Format("network.dns.presetChosen", preset.Name);
		}
	}

	[RelayCommand]
	private void UseBenchmarkResult(DnsBenchmarkRowViewModel? row)
	{
		if (row != null && row.Result.MedianMs.HasValue)
		{
			UseServers(row.Result.Address, row.Result.Secondary ?? string.Empty);
			BenchmarkStatus = AppServices.Localizer.Format("network.dns.presetChosen", row.Result.Name);
		}
	}

	private void UseServers(string primary, string secondary)
	{
		PrimaryDns = primary;
		SecondaryDns = secondary;
		Tweaks.RefreshStateAsync();
	}

	private bool CanRunBenchmark()
	{
		return !IsBenchmarking;
	}

	[RelayCommand(CanExecute = "CanRunBenchmark")]
	private async Task RunBenchmarkAsync()
	{
		IsBenchmarking = true;
		BenchmarkResults.Clear();
		ILocalizer loc = AppServices.Localizer;
		try
		{
			Progress<int> progress = new Progress<int>((int p) =>
			{
				BenchmarkStatus = loc.Format("network.dns.benchmark.running", p);
			});
			IReadOnlyList<DnsBenchmarkResult> readOnlyList = await DnsBenchmark.RunAsync(loc["network.dns.benchmark.current"], progress).ConfigureAwait(continueOnCapturedContext: true);
			DnsBenchmarkResult dnsBenchmarkResult = readOnlyList.FirstOrDefault((DnsBenchmarkResult r) => r.MedianMs.HasValue);
			foreach (DnsBenchmarkResult item in readOnlyList)
			{
				BenchmarkResults.Add(new DnsBenchmarkRowViewModel(item, (object)item == dnsBenchmarkResult));
			}
			BenchmarkStatus = (((object)dnsBenchmarkResult == null) ? loc["network.dns.benchmark.failed"] : loc.Format("network.dns.benchmark.done", dnsBenchmarkResult.Name, dnsBenchmarkResult.MedianMs.Value));
		}
		catch (Exception exception)
		{
			AppServices.Logger.Error("DNS benchmark failed", exception);
			BenchmarkStatus = loc["network.dns.benchmark.failed"];
		}
		finally
		{
			IsBenchmarking = false;
		}
	}

	public Task ActivatedAsync()
	{
		return Tweaks.ActivatedAsync();
	}

	public void OnLanguageChanged()
	{
		Tweaks.OnLanguageChanged();
		Tools.OnLanguageChanged();
		OnPropertyChanged("DnsWarning");
		OnPropertyChanged("DnsTitle");
		OnPropertyChanged("PrimaryLabel");
		OnPropertyChanged("SecondaryLabel");
		BenchmarkStatus = string.Empty;
		foreach (DnsBenchmarkRowViewModel benchmarkResult in BenchmarkResults)
		{
			benchmarkResult.OnLanguageChanged();
		}
		DnsValidationMessage = string.Empty;
		Persist();
	}

	private void Persist()
	{
		if (!_initialized)
		{
			return;
		}
		string primary = PrimaryDns?.Trim() ?? string.Empty;
		string secondary = SecondaryDns?.Trim() ?? string.Empty;
		if (!IsValid(primary))
		{
			DnsValidationMessage = AppServices.Localizer["network.dns.invalidPrimary"];
			return;
		}
		if (secondary.Length > 0 && !IsValid(secondary))
		{
			DnsValidationMessage = AppServices.Localizer["network.dns.invalidSecondary"];
			return;
		}
		DnsValidationMessage = string.Empty;
		_settings.Update((AppSettings s) => s with
		{
			PrimaryDns = primary,
			SecondaryDns = secondary
		});
	}

	public static bool IsValid(string? address)
	{
		if (IPAddress.TryParse(address?.Trim(), out IPAddress address2))
		{
			return address2.AddressFamily == AddressFamily.InterNetwork;
		}
		return false;
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnPrimaryDnsChanged(string value)
	{
		Persist();
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnSecondaryDnsChanged(string value)
	{
		Persist();
	}
}
