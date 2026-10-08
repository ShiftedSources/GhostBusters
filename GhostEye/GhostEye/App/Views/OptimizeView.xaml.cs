using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using GhostEye.App.ViewModels;

namespace GhostEye.App.Views;

public partial class OptimizeView : UserControl, IComponentConnector, IStyleConnector
{
	public OptimizeView()
	{
		InitializeComponent();
	}

	private void OnPresetClick(object sender, RoutedEventArgs e)
	{
		if (!(sender is ToggleButton { Tag: PresetViewModel tag } toggleButton) || !(DataContext is OptimizeViewModel optimizeViewModel))
		{
			return;
		}
		foreach (ToggleButton item in FindPresetButtons())
		{
			if (item != toggleButton)
			{
				item.IsChecked = false;
			}
		}
		optimizeViewModel.SelectedPreset = ((toggleButton.IsChecked == true) ? tag : null);
	}

	private IEnumerable<ToggleButton> FindPresetButtons()
	{
		return Walk(this).OfType<ToggleButton>();
	}

	private static IEnumerable<DependencyObject> Walk(DependencyObject root)
	{
		int count = VisualTreeHelper.GetChildrenCount(root);
		for (int i = 0; i < count; i++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(root, i);
			if (child is ToggleButton toggleButton && toggleButton.Tag is PresetViewModel)
			{
				yield return child;
			}
			foreach (DependencyObject item in Walk(child))
			{
				yield return item;
			}
		}
	}

}
