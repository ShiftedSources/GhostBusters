using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using GhostEye.App.ViewModels;

namespace GhostEye.App.Controls;

public partial class TweakCategoryPanel : UserControl, IComponentConnector
{
	public static readonly DependencyProperty CategoryProperty = DependencyProperty.Register("Category", typeof(TweakCategoryViewModel), typeof(TweakCategoryPanel), new PropertyMetadata(null, (DependencyObject d, DependencyPropertyChangedEventArgs _) =>
	{
		((TweakCategoryPanel)d).UpdateChevron(animate: false);
	}));

	public TweakCategoryViewModel? Category
	{
		get
		{
			return (TweakCategoryViewModel)GetValue(CategoryProperty);
		}
		set
		{
			SetValue(CategoryProperty, value);
		}
	}

	public TweakCategoryPanel()
	{
		InitializeComponent();
		Loaded += (object _, RoutedEventArgs _) =>
		{
			UpdateChevron(animate: false);
		};
	}

	private void OnHeaderClick(object sender, RoutedEventArgs e)
	{
		if (Category != null)
		{
			Category.IsExpanded = !Category.IsExpanded;
			UpdateChevron(animate: true);
		}
	}

	private void UpdateChevron(bool animate)
	{
		TweakCategoryViewModel? category = Category;
		int num = ((category != null && category.IsExpanded) ? 180 : 0);
		if (!animate)
		{
			ChevronRotation.BeginAnimation(RotateTransform.AngleProperty, null);
			ChevronRotation.Angle = num;
		}
		else
		{
			ChevronRotation.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(num, new Duration(TimeSpan.FromMilliseconds(160.0)))
			{
				EasingFunction = new CubicEase
				{
					EasingMode = EasingMode.EaseOut
				}
			});
		}
	}
}
