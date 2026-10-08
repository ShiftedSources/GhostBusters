using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using GhostEye.App.Services;

namespace GhostEye.App;

public partial class ShellView : UserControl, IComponentConnector
{
	private ShellWindow? Owner => Window.GetWindow(this) as ShellWindow;

	public ShellView()
	{
		InitializeComponent();
		UpdatePumpkin();
		Loaded += (object _, RoutedEventArgs _) =>
		{
			ThemeManager.SeasonChanged += OnSeasonChanged;
		};
		Unloaded += (object _, RoutedEventArgs _) =>
		{
			ThemeManager.SeasonChanged -= OnSeasonChanged;
		};
	}

	private void OnSeasonChanged(object? sender, EventArgs e)
	{
		UpdatePumpkin();
	}

	private void UpdatePumpkin()
	{
		Pumpkin.Visibility = ((!ThemeManager.IsHalloweenSeason) ? Visibility.Collapsed : Visibility.Visible);
	}

	private void OnTitleBarDrag(object sender, MouseButtonEventArgs e)
	{
		Owner?.OnTitleBarDrag(sender, e);
	}

	private void OnMinimize(object sender, RoutedEventArgs e)
	{
		Owner?.OnMinimize(sender, e);
	}

	private void OnMaximize(object sender, RoutedEventArgs e)
	{
		Owner?.OnMaximize(sender, e);
	}

	private void OnClose(object sender, RoutedEventArgs e)
	{
		Owner?.OnClose(sender, e);
	}

	private void OnPumpkinClick(object sender, RoutedEventArgs e)
	{
		Owner?.OnPumpkinClicked();
	}

}
