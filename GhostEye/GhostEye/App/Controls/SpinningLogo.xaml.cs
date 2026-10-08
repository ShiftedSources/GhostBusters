using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GhostEye.App.Services;

namespace GhostEye.App.Controls;

public partial class SpinningLogo : UserControl, IComponentConnector
{
	private const int FrameCount = 180;

	private const double ShineHalfWidth = 40.0;

	private static readonly Dictionary<int, Task<BitmapSource[]>> Turns = new Dictionary<int, Task<BitmapSource[]>>();

	private readonly Stopwatch _clock = new Stopwatch();

	private BitmapSource[]? _frames;

	private int _shownFrame = -1;

	private bool _running;

	private bool _paused;

	private TimeSpan _lastDraw = TimeSpan.FromHours(-1.0);

	private static readonly TimeSpan FrameInterval = TimeSpan.FromMilliseconds(33.0);

	public static readonly DependencyProperty SecondsPerTurnProperty = DependencyProperty.Register("SecondsPerTurn", typeof(double), typeof(SpinningLogo), new PropertyMetadata(6.0));

	public double SecondsPerTurn
	{
		get
		{
			return (double)GetValue(SecondsPerTurnProperty);
		}
		set
		{
			SetValue(SecondsPerTurnProperty, value);
		}
	}

	public SpinningLogo()
	{
		InitializeComponent();
		double faceScale = CoinRenderer.FaceScale;
		PlaceholderScale.ScaleX = faceScale;
		PlaceholderScale.ScaleY = faceScale;
		Loaded += async (object _, RoutedEventArgs _) =>
		{
			await LoadFramesAsync();
			Start();
		};
		Unloaded += (object _, RoutedEventArgs _) =>
		{
			Stop();
		};
		IsVisibleChanged += (object _, DependencyPropertyChangedEventArgs _) =>
		{
			if (IsVisible)
			{
				Start();
			}
			else
			{
				Stop();
			}
		};
		Loaded += (object _, RoutedEventArgs _) =>
		{
			RenderBudget.Changed += OnBudgetChanged;
		};
		Unloaded += (object _, RoutedEventArgs _) =>
		{
			RenderBudget.Changed -= OnBudgetChanged;
		};
	}

	public void SetAnimationsPaused(bool paused)
	{
		_paused = paused;
		if (paused)
		{
			Stop();
		}
		else
		{
			Start();
		}
	}

	private void OnBudgetChanged(object? sender, EventArgs e)
	{
		if (RenderBudget.AllowsAnimations)
		{
			Start();
			return;
		}
		Stop();
		ShowFrame(0);
		Shine.Opacity = 0.0;
	}

	private async Task LoadFramesAsync()
	{
		if (_frames != null)
		{
			return;
		}
		DpiScale dpi = VisualTreeHelper.GetDpi(this);
		int num = (int)Math.Round(Math.Max(ActualWidth, ActualHeight) * dpi.DpiScaleX);
		if (num > 0)
		{
			if (!Turns.TryGetValue(num, out Task<BitmapSource[]> value))
			{
				value = RenderOnStaThread(num, dpi.PixelsPerInchX);
				Turns[num] = value;
			}
			try
			{
				_frames = await value;
			}
			catch (Exception)
			{
				return;
			}
			PlaceholderScale.ScaleX = 1.0;
			PlaceholderScale.ScaleY = 1.0;
			ShowFrame(0);
		}
	}

	private static Task<BitmapSource[]> RenderOnStaThread(int pixels, double dpi)
	{
		TaskCompletionSource<BitmapSource[]> completion = new TaskCompletionSource<BitmapSource[]>();
		Thread thread = new Thread(() =>
		{
			try
			{
				completion.SetResult(CoinRenderer.LoadOrRenderTurn(pixels, 180, dpi));
			}
			catch (Exception exception)
			{
				completion.SetException(exception);
			}
		});
		thread.IsBackground = true;
		thread.Name = "SpinningLogo render";
		thread.Priority = ThreadPriority.BelowNormal;
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		return completion.Task;
	}

	private void ShowFrame(int index)
	{
		if (_frames != null && index != _shownFrame)
		{
			_shownFrame = index;
			Frame.Source = _frames[index];
			ShineMask.ImageSource = _frames[index];
		}
	}

	private void OnRendering(object? sender, EventArgs e)
	{
		if (!(_clock.Elapsed - _lastDraw < FrameInterval))
		{
			_lastDraw = _clock.Elapsed;
			double num = Math.Max(0.5, SecondsPerTurn);
			double num2 = _clock.Elapsed.TotalSeconds / num;
			double num3 = num2 - Math.Floor(num2);
			ShowFrame((int)(num3 * 180.0) % 180);
			UpdateShine(num3 * 360.0);
		}
	}

	private void UpdateShine(double angle)
	{
		double num = (angle + 90.0) % 180.0 - 90.0;
		if (Math.Abs(num) >= 40.0)
		{
			Shine.Opacity = 0.0;
			return;
		}
		double num2 = num / 40.0 * 0.6;
		ShineOffset.X = num2;
		ShineOffset.Y = num2;
		Shine.Opacity = 1.0;
	}

	private void Start()
	{
		if (!_running && !_paused && _frames != null && IsLoaded && IsVisible && RenderBudget.AllowsAnimations)
		{
			_clock.Start();
			CompositionTarget.Rendering += OnRendering;
			_running = true;
		}
	}

	private void Stop()
	{
		if (_running)
		{
			CompositionTarget.Rendering -= OnRendering;
			_clock.Stop();
			_running = false;
		}
	}
}
