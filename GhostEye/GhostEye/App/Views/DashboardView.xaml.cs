using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using GhostEye.App.ViewModels;

namespace GhostEye.App.Views;

public partial class DashboardView : UserControl, IComponentConnector
{
	private DashboardViewModel? _viewModel;

	public DashboardView()
	{
		InitializeComponent();
		DataContextChanged += OnDataContextChanged;
		Loaded += (object _, RoutedEventArgs _) =>
		{
			_viewModel?.StartLiveCounters();
		};
		Unloaded += (object _, RoutedEventArgs _) =>
		{
			_viewModel?.StopLiveCounters();
			Detach();
		};
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		Detach();
		_viewModel = e.NewValue as DashboardViewModel;
		if (_viewModel != null)
		{
			_viewModel.PropertyChanged += OnViewModelPropertyChanged;
			if (IsLoaded)
			{
				_viewModel.StartLiveCounters();
			}
		}
		UpdateScoreArc();
	}

	private void Detach()
	{
		if (_viewModel != null)
		{
			_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
			_viewModel = null;
		}
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		string propertyName = e.PropertyName;
		if ((propertyName == "Score" || propertyName == "HasScore") ? true : false)
		{
			UpdateScoreArc();
		}
	}

	private void UpdateScoreArc()
	{
		DashboardViewModel viewModel = _viewModel;
		if (viewModel == null || !viewModel.HasScore || _viewModel.Score <= 0)
		{
			ScoreArc.Data = null;
			return;
		}
		double num = Math.Min(Math.Clamp((double)_viewModel.Score / 100.0, 0.0, 1.0) * 360.0, 359.9);
		double num2 = (num - 90.0) * Math.PI / 180.0;
		Point startPoint = new Point(88.0, 14.0);
		Point point = new Point(88.0 + 74.0 * Math.Cos(num2), 88.0 + 74.0 * Math.Sin(num2));
		PathFigure pathFigure = new PathFigure
		{
			StartPoint = startPoint,
			IsClosed = false,
			IsFilled = false
		};
		pathFigure.Segments.Add(new ArcSegment
		{
			Point = point,
			Size = new Size(74.0, 74.0),
			SweepDirection = SweepDirection.Clockwise,
			IsLargeArc = (num > 180.0)
		});
		PathGeometry pathGeometry = new PathGeometry();
		pathGeometry.Figures.Add(pathFigure);
		ScoreArc.Data = pathGeometry;
	}
}
