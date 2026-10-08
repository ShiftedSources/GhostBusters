using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GhostEye.Core.Services;

namespace GhostEye.App.Services;

public static class ThemeManager
{
	private static readonly Dictionary<string, Color> Violet = new Dictionary<string, Color>
	{
		["Bg0"] = Hex("#09070F"),
		["Bg1"] = Hex("#100B1A"),
		["Panel"] = Hex("#171022"),
		["PanelSunk"] = Hex("#130D1D"),
		["PanelHover"] = Hex("#1C142A"),
		["Line"] = Hex("#271C3A"),
		["LineSoft"] = Hex("#1E1630"),
		["LineHover"] = Hex("#3B2B56"),
		["Accent"] = Hex("#B44DFE"),
		["AccentBright"] = Hex("#D8A8FF"),
		["AccentDim"] = Hex("#6E4E96"),
		["Text"] = Hex("#EBE6F5"),
		["TextDim"] = Hex("#9A90AC"),
		["ArcEnd"] = Hex("#8B3FD4"),
		["Sidebar"] = Hex("#8C0B0714")
	};

	private static readonly Dictionary<string, Color> Pumpkin = new Dictionary<string, Color>
	{
		["Bg0"] = Hex("#0D0806"),
		["Bg1"] = Hex("#160C08"),
		["Panel"] = Hex("#1E120B"),
		["PanelSunk"] = Hex("#190F09"),
		["PanelHover"] = Hex("#27170D"),
		["Line"] = Hex("#3B2415"),
		["LineSoft"] = Hex("#2B1A10"),
		["LineHover"] = Hex("#58351C"),
		["Accent"] = Hex("#FF8A1F"),
		["AccentBright"] = Hex("#FFC27A"),
		["AccentDim"] = Hex("#9C5A24"),
		["Text"] = Hex("#F6EDE4"),
		["TextDim"] = Hex("#AE9B8B"),
		["ArcEnd"] = Hex("#D9601A"),
		["Sidebar"] = Hex("#8C120A06")
	};

	private static DispatcherTimer? _seasonTimer;

	private static bool _inSeason;

	public static bool IsPumpkin { get; private set; }

	public static bool IsHalloweenSeason => HalloweenSeason.IsSeasonAt(DateTime.UtcNow);

	public static event EventHandler? Changed;

	public static event EventHandler? SeasonChanged;

	public static void StartSeasonWatch()
	{
		_inSeason = IsHalloweenSeason;
		_seasonTimer = new DispatcherTimer();
		_seasonTimer.Tick += (object? _, EventArgs _) =>
		{
			CheckSeason();
		};
		ArmSeasonTimer();
	}

	private static void ArmSeasonTimer()
	{
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan = HalloweenSeason.NextRomeMidnightUtc(utcNow) - utcNow + TimeSpan.FromSeconds(2.0);
		_seasonTimer.Interval = ((timeSpan < TimeSpan.FromHours(1.0)) ? timeSpan : TimeSpan.FromHours(1.0));
		_seasonTimer.Start();
	}

	private static void CheckSeason()
	{
		_seasonTimer.Stop();
		bool isHalloweenSeason = IsHalloweenSeason;
		if (isHalloweenSeason != _inSeason)
		{
			_inSeason = isHalloweenSeason;
			AppServices.Logger.Info("Halloween season " + (isHalloweenSeason ? "started" : "ended") + ".");
			SeasonChanged?.Invoke(null, EventArgs.Empty);
		}
		ArmSeasonTimer();
	}

	public static void ResetToViolet(ShellWindow window)
	{
		if (IsPumpkin)
		{
			BitmapSource oldLook = window.SnapshotView();
			IsPumpkin = false;
			WriteResources();
			AppServices.Logger.Info("Theme switched to violet (end of season).");
			window.RebuildWithTheme(oldLook, new Point(0.0, 0.0));
			Changed?.Invoke(null, EventArgs.Empty);
		}
	}

	public static Color C(string key)
	{
		return (IsPumpkin ? Pumpkin : Violet)[key];
	}

	public static void ApplyAtStartup()
	{
		if (AppServices.Settings.Current.HalloweenTheme && IsHalloweenSeason)
		{
			IsPumpkin = true;
			WriteResources();
		}
	}

	public static void Toggle(ShellWindow window, Point origin)
	{
		BitmapSource oldLook = window.SnapshotView();
		IsPumpkin = !IsPumpkin;
		AppServices.Settings.Update((AppSettings s) => s with
		{
			HalloweenTheme = IsPumpkin
		});
		WriteResources();
		AppServices.Logger.Info("Theme switched to " + (IsPumpkin ? "pumpkin" : "violet") + ".");
		window.RebuildWithTheme(oldLook, origin);
		Changed?.Invoke(null, EventArgs.Empty);
	}

	private static void WriteResources()
	{
		ResourceDictionary resources = Application.Current.Resources;
		Dictionary<string, Color> dictionary = (IsPumpkin ? Pumpkin : Violet);
		string[] array = new string[13]
		{
			"Bg0", "Bg1", "Panel", "PanelSunk", "PanelHover", "Line", "LineSoft", "LineHover", "Accent", "AccentBright",
			"AccentDim", "Text", "TextDim"
		};
		foreach (string text in array)
		{
			resources[text] = dictionary[text];
			resources[text + "Brush"] = Frozen(new SolidColorBrush(dictionary[text]));
		}
		resources["AccentWashBrush"] = Frozen(new SolidColorBrush(dictionary["Accent"])
		{
			Opacity = 0.1
		});
		resources["AccentWashStrongBrush"] = Frozen(new SolidColorBrush(dictionary["Accent"])
		{
			Opacity = 0.16
		});
		resources["SidebarBrush"] = Frozen(new SolidColorBrush(dictionary["Sidebar"]));
		resources["ScoreArcBrush"] = Frozen(new LinearGradientBrush(dictionary["AccentBright"], dictionary["ArcEnd"], new Point(0.0, 0.0), new Point(1.0, 1.0)));
		Collection<ResourceDictionary> mergedDictionaries = resources.MergedDictionaries;
		for (int j = 0; j < mergedDictionaries.Count; j++)
		{
			string text2 = mergedDictionaries[j].Source?.OriginalString ?? string.Empty;
			if (text2.EndsWith("Controls.xaml", StringComparison.OrdinalIgnoreCase) || text2.EndsWith("Typography.xaml", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = text2.Substring(text2.LastIndexOf('/') + 1);
				mergedDictionaries[j] = new ResourceDictionary
				{
					Source = new Uri("pack://application:,,,/GhostEye;component/Themes/" + text3, UriKind.Absolute)
				};
			}
		}
	}

	private static T Frozen<T>(T freezable) where T : Freezable
	{
		freezable.Freeze();
		return freezable;
	}

	private static Color Hex(string value)
	{
		return (Color)ColorConverter.ConvertFromString(value);
	}
}
