using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using GhostEye.App.Services;
using GhostEye.App.ViewModels;

namespace GhostEye.App;

public partial class ShellWindow : Window, IComponentConnector
{
	private struct Rect
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	private struct MonitorInfo
	{
		public int cbSize;

		public Rect rcMonitor;

		public Rect rcWork;

		public int dwFlags;
	}

	private struct PointInt
	{
		public int X;

		public int Y;
	}

	private struct MinMaxInfo
	{
		public PointInt ptReserved;

		public PointInt ptMaxSize;

		public PointInt ptMaxPosition;

		public PointInt ptMinTrackSize;

		public PointInt ptMaxTrackSize;
	}

	private readonly ShellViewModel _viewModel;

	private bool _themeSwitching;

	private const int WmGetMinMaxInfo = 36;

	private const int MonitorDefaultToNearest = 2;

	internal ShellView View { get; private set; }

	public ShellWindow()
	{
		_viewModel = new ShellViewModel();
		AppServices.Shell = _viewModel;
		DataContext = _viewModel;
		InitializeComponent();
		View = new ShellView();
		ViewHost.Content = View;
		Snow.IsSnowEnabled = AppServices.Settings.Current.SnowEnabled;
		Root.PreviewMouseMove += OnPointerMoved;
		_viewModel.PropertyChanged += OnViewModelChanged;
		SourceInitialized += OnSourceInitialized;
		Closing += OnWindowClosing;
		StateChanged += OnWindowStateChanged;
		ThemeManager.SeasonChanged += OnSeasonChanged;
	}

	private void OnSeasonChanged(object? sender, EventArgs e)
	{
		if (!ThemeManager.IsHalloweenSeason && !_themeSwitching)
		{
			ThemeManager.ResetToViolet(this);
		}
	}

	private void OnWindowStateChanged(object? sender, EventArgs e)
	{
		bool animationsPaused = WindowState == WindowState.Minimized;
		Snow.SetAnimationsPaused(animationsPaused);
		View.Logo.SetAnimationsPaused(animationsPaused);
	}

	private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == "CurrentPage")
		{
			PlayPageTransition();
		}
	}

	internal void OnPumpkinClicked()
	{
		if (!_themeSwitching)
		{
			Button pumpkin = View.Pumpkin;
			Point origin = pumpkin.TranslatePoint(new Point(pumpkin.ActualWidth / 2.0, pumpkin.ActualHeight / 2.0), Root);
			ThemeManager.Toggle(this, origin);
		}
	}

	internal BitmapSource SnapshotView()
	{
		DpiScale dpi = VisualTreeHelper.GetDpi(View);
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(Math.Max(1, (int)Math.Ceiling(View.ActualWidth * dpi.DpiScaleX)), Math.Max(1, (int)Math.Ceiling(View.ActualHeight * dpi.DpiScaleY)), dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
		renderTargetBitmap.Render(View);
		renderTargetBitmap.Freeze();
		return renderTargetBitmap;
	}

	internal void RebuildWithTheme(BitmapSource oldLook, Point origin)
	{
		_themeSwitching = true;
		Background = (Brush)FindResource("Bg0Brush");
		Root.SetValue(TextElement.ForegroundProperty, FindResource("TextBrush"));
		Snow.SwitchTheme();
		double num = Math.Max(1.0, Root.ActualWidth);
		double num2 = Math.Max(1.0, Root.ActualHeight);
		double reach = Math.Sqrt(Math.Pow(Math.Max(origin.X, num - origin.X), 2.0) + Math.Pow(Math.Max(origin.Y, num2 - origin.Y), 2.0)) + 40.0;
		var (oldMask, oldInner, oldOuter) = CircleMask(origin, Colors.Transparent, Colors.Black);
		var (newMask, newInner, newOuter) = CircleMask(origin, Colors.Black, Colors.Transparent);
		ThemeOverlay.Source = oldLook;
		ThemeOverlay.OpacityMask = oldMask;
		ThemeOverlay.Visibility = Visibility.Visible;
		ShellView fresh = new ShellView
		{
			OpacityMask = newMask
		};
		View = fresh;
		ViewHost.Content = fresh;
		DateTime started = DateTime.UtcNow;
		TimeSpan duration = TimeSpan.FromMilliseconds(1100.0);
		CompositionTarget.Rendering += Step;
		void Step(object? sender, EventArgs e)
		{
			double num3 = Math.Clamp((DateTime.UtcNow - started) / duration, 0.0, 1.0);
			double num4 = ((num3 < 0.5) ? (4.0 * num3 * num3 * num3) : (1.0 - Math.Pow(-2.0 * num3 + 2.0, 3.0) / 2.0));
			double num5 = Math.Max(1.0, num4 * (reach + 48.0));
			RadialGradientBrush[] array = new RadialGradientBrush[2] { oldMask, newMask };
			foreach (RadialGradientBrush obj in array)
			{
				obj.RadiusX = num5;
				obj.RadiusY = num5;
			}
			double num6 = Math.Clamp((num5 - 48.0) / num5, 0.0, 1.0);
			GradientStop gradientStop = oldInner;
			double offset = (newInner.Offset = num6);
			gradientStop.Offset = offset;
			GradientStop gradientStop2 = oldOuter;
			offset = (newOuter.Offset = 1.0);
			gradientStop2.Offset = offset;
			if (!(num3 < 1.0))
			{
				CompositionTarget.Rendering -= Step;
				ThemeOverlay.Visibility = Visibility.Collapsed;
				ThemeOverlay.Source = null;
				ThemeOverlay.OpacityMask = null;
				fresh.OpacityMask = null;
				_themeSwitching = false;
			}
		}
	}

	private static (RadialGradientBrush Mask, GradientStop Inner, GradientStop Outer) CircleMask(Point origin, Color inside, Color outside)
	{
		GradientStop gradientStop = new GradientStop(inside, 0.0);
		GradientStop gradientStop2 = new GradientStop(outside, 1.0);
		return (Mask: new RadialGradientBrush
		{
			MappingMode = BrushMappingMode.Absolute,
			Center = origin,
			GradientOrigin = origin,
			RadiusX = 1.0,
			RadiusY = 1.0,
			GradientStops = 
			{
				new GradientStop(inside, 0.0),
				gradientStop,
				gradientStop2
			}
		}, Inner: gradientStop, Outer: gradientStop2);
	}

	private void OnWindowClosing(object? sender, CancelEventArgs e)
	{
		if (App.IsExiting)
		{
			return;
		}
		e.Cancel = true;
		if (!AppServices.Settings.Current.RunInBackground)
		{
			Dispatcher.BeginInvoke(new Action(App.RequestExit));
			return;
		}
		Hide();
		if (!AppServices.Settings.Current.BackgroundHintShown)
		{
			AppServices.Settings.Update((AppSettings s) => s with
			{
				BackgroundHintShown = true
			});
			TrayIcon.Notify(AppServices.Localizer["tray.hint.title"], AppServices.Localizer["tray.hint.body"]);
		}
	}

	private void OnPointerMoved(object sender, MouseEventArgs e)
	{
		Point position = e.GetPosition(Root);
		if (!(Root.ActualWidth <= 0.0) && !(Root.ActualHeight <= 0.0))
		{
			Snow.SetParallax(position.X / Root.ActualWidth - 0.5, position.Y / Root.ActualHeight - 0.5);
		}
	}

	private void PlayPageTransition()
	{
		Storyboard storyboard = new Storyboard();
		DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(180.0)))
		{
			EasingFunction = new CubicEase
			{
				EasingMode = EasingMode.EaseOut
			}
		};
		Storyboard.SetTarget(doubleAnimation, View.PageHost);
		Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(UIElement.OpacityProperty));
		storyboard.Children.Add(doubleAnimation);
		DoubleAnimation doubleAnimation2 = new DoubleAnimation(12.0, 0.0, new Duration(TimeSpan.FromMilliseconds(180.0)))
		{
			EasingFunction = new CubicEase
			{
				EasingMode = EasingMode.EaseOut
			}
		};
		Storyboard.SetTarget(doubleAnimation2, View.PageHost);
		Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("RenderTransform.Y"));
		storyboard.Children.Add(doubleAnimation2);
		storyboard.Begin();
	}

	public void SetSnowEnabled(bool enabled)
	{
		Snow.IsSnowEnabled = enabled;
	}

	internal void OnTitleBarDrag(object sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount == 2)
		{
			ToggleMaximized();
		}
		else if (e.ButtonState == MouseButtonState.Pressed)
		{
			DragMove();
		}
	}

	internal void OnMinimize(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	internal void OnMaximize(object sender, RoutedEventArgs e)
	{
		ToggleMaximized();
	}

	internal void OnClose(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void ToggleMaximized()
	{
		WindowState = ((WindowState != WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal);
	}

	private void OnSourceInitialized(object? sender, EventArgs e)
	{
		nint handle = new WindowInteropHelper(this).Handle;
		HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
	}

	private static nint WindowProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg != 36)
		{
			return IntPtr.Zero;
		}
		nint num = MonitorFromWindow(hwnd, 2);
		if (num == IntPtr.Zero)
		{
			return IntPtr.Zero;
		}
		MonitorInfo lpmi = new MonitorInfo
		{
			cbSize = Marshal.SizeOf<MonitorInfo>()
		};
		if (!GetMonitorInfo(num, ref lpmi))
		{
			return IntPtr.Zero;
		}
		MinMaxInfo structure = Marshal.PtrToStructure<MinMaxInfo>(lParam);
		Rect rcWork = lpmi.rcWork;
		Rect rcMonitor = lpmi.rcMonitor;
		structure.ptMaxPosition.X = rcWork.left - rcMonitor.left;
		structure.ptMaxPosition.Y = rcWork.top - rcMonitor.top;
		structure.ptMaxSize.X = rcWork.right - rcWork.left;
		structure.ptMaxSize.Y = rcWork.bottom - rcWork.top;
		Marshal.StructureToPtr(structure, lParam, fDeleteOld: true);
		handled = true;
		return IntPtr.Zero;
	}

	[DllImport("user32.dll")]
	private static extern nint MonitorFromWindow(nint hwnd, int flags);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool GetMonitorInfo(nint hMonitor, ref MonitorInfo lpmi);
}
