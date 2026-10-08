using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using GhostEye.App.Services;

namespace GhostEye.App.Controls;

public sealed class SnowBackground : FrameworkElement
{
	private enum Kind
	{
		Flake,
		SoftFlake,
		Ember,
		Bat,
		Ghost,
		Candy,
		CandyCorn
	}

	private sealed record LayerSpec(int Count, double MinSize, double MaxSize, double MinSpeed, double MaxSpeed, double Opacity, double Parallax, Kind[] Kinds);

	private sealed class Particle
	{
		public required LayerSpec Layer { get; init; }

		public required Kind Kind { get; init; }

		public required double Size { get; init; }

		public double X { get; set; }

		public double Y { get; set; }

		public required double FallSpeed { get; init; }

		public required double DriftAmplitude { get; init; }

		public required double DriftFrequency { get; init; }

		public required double Phase { get; init; }

		public required double Spin { get; init; }
	}

	private sealed class Scene
	{
		public required bool Pumpkin { get; init; }

		public required Brush Backdrop { get; init; }

		public required Brush GlowLeft { get; init; }

		public required Brush GlowRight { get; init; }

		public List<Particle> Particles { get; } = new List<Particle>();
	}

	private static class Sprites
	{
		public static readonly Brush Flake = Solid(Color.FromRgb(235, 226, byte.MaxValue));

		public static readonly Brush SoftFlake = Frozen(new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop(Color.FromArgb(byte.MaxValue, 232, 218, byte.MaxValue), 0.0),
				new GradientStop(Color.FromArgb(120, 205, 165, byte.MaxValue), 0.55),
				new GradientStop(Color.FromArgb(0, 180, 120, byte.MaxValue), 1.0)
			}
		});

		public static readonly Brush Ember = Frozen(new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop(Color.FromArgb(byte.MaxValue, byte.MaxValue, 214, 150), 0.0),
				new GradientStop(Color.FromArgb(150, byte.MaxValue, 138, 31), 0.35),
				new GradientStop(Color.FromArgb(0, byte.MaxValue, 100, 20), 1.0)
			}
		});

		public static readonly Drawing Bat = Shape("M12,9.5 C12.8,8.2 13.6,8 14.2,8.6 L14.6,7.4 L15.1,8.9 C17,8.2 19.6,8.3 23,10.6 C21.2,10.8 20.1,11.9 19.8,13.5 C18.8,12.6 17.6,12.5 16.6,13.4 C15.8,12.6 14.8,12.6 14.1,13.6 C13.6,14.4 12.8,14.9 12,15.2 C11.2,14.9 10.4,14.4 9.9,13.6 C9.2,12.6 8.2,12.6 7.4,13.4 C6.4,12.5 5.2,12.6 4.2,13.5 C3.9,11.9 2.8,10.8 1,10.6 C4.4,8.3 7,8.2 8.9,8.9 L9.4,7.4 L9.8,8.6 C10.4,8 11.2,8.2 12,9.5 Z", Color.FromRgb(58, 31, 63), Color.FromRgb(byte.MaxValue, 138, 31), 0.7, new Point[2]
		{
			new Point(11.2, 10.6),
			new Point(12.8, 10.6)
		}, Color.FromRgb(byte.MaxValue, 194, 122), 0.55);

		public static readonly Drawing Ghost = Shape("M5,21 L5,11 C5,6.6 8.1,3.5 12,3.5 C15.9,3.5 19,6.6 19,11 L19,21 L16.7,19 L14.3,21 L12,19 L9.7,21 L7.3,19 Z", Color.FromArgb(235, 246, 237, 228), Color.FromArgb(120, byte.MaxValue, 194, 122), 0.6, new Point[2]
		{
			new Point(9.6, 10.5),
			new Point(14.4, 10.5)
		}, Color.FromRgb(42, 18, 4), 1.2);

		public static readonly Drawing Candy = BuildCandy();

		public static readonly Drawing CandyCorn = BuildCandyCorn();

		private static Brush Solid(Color color)
		{
			return Frozen(new SolidColorBrush(color));
		}

		private static Drawing Shape(string data, Color fill, Color rim, double rimWidth, Point[] eyes, Color eyeColor, double eyeRadius)
		{
			DrawingGroup drawingGroup = new DrawingGroup();
			using (DrawingContext drawingContext = drawingGroup.Open())
			{
				drawingContext.DrawGeometry(Solid(fill), Frozen(new Pen(Solid(rim), rimWidth)), Geometry.Parse(data));
				foreach (Point center in eyes)
				{
					drawingContext.DrawEllipse(Solid(eyeColor), null, center, eyeRadius, eyeRadius * 1.25);
				}
			}
			drawingGroup.Freeze();
			return drawingGroup;
		}

		private static Drawing BuildCandy()
		{
			DrawingGroup drawingGroup = new DrawingGroup();
			using (DrawingContext drawingContext = drawingGroup.Open())
			{
				Brush brush = Solid(Color.FromRgb(180, 77, 254));
				drawingContext.DrawGeometry(brush, null, Geometry.Parse("M7,12 L2,8 L2.8,12 L2,16 Z M17,12 L22,8 L21.2,12 L22,16 Z"));
				drawingContext.DrawEllipse(Solid(Color.FromRgb(byte.MaxValue, 138, 31)), null, new Point(12.0, 12.0), 5.5, 4.4);
				Pen pen = Frozen(new Pen(Solid(Color.FromRgb(byte.MaxValue, 224, 176)), 1.2));
				drawingContext.DrawLine(pen, new Point(9.6, 8.8), new Point(11.4, 15.2));
				drawingContext.DrawLine(pen, new Point(12.6, 8.8), new Point(14.4, 15.2));
			}
			drawingGroup.Freeze();
			return drawingGroup;
		}

		private static Drawing BuildCandyCorn()
		{
			DrawingGroup drawingGroup = new DrawingGroup();
			using (DrawingContext drawingContext = drawingGroup.Open())
			{
				drawingContext.DrawGeometry(Solid(Color.FromRgb(byte.MaxValue, 200, 58)), null, Geometry.Parse("M4.5,19 C8,21 16,21 19.5,19 L17.3,14.8 C14,16 10,16 6.7,14.8 Z"));
				drawingContext.DrawGeometry(Solid(Color.FromRgb(byte.MaxValue, 122, 26)), null, Geometry.Parse("M6.7,14.8 C10,16 14,16 17.3,14.8 L15,10 C13,10.7 11,10.7 9,10 Z"));
				drawingContext.DrawGeometry(Solid(Color.FromRgb(251, 244, 232)), null, Geometry.Parse("M9,10 C11,10.7 13,10.7 15,10 L12,3.5 Z"));
			}
			drawingGroup.Freeze();
			return drawingGroup;
		}
	}

	private static readonly LayerSpec[] SnowLayers = new LayerSpec[3]
	{
		new LayerSpec(34, 1.1, 2.1, 26.0, 40.0, 0.85, 26.0, new Kind[1]),
		new LayerSpec(26, 2.3, 3.5, 16.0, 26.0, 0.42, 15.0, new Kind[1] { Kind.SoftFlake }),
		new LayerSpec(18, 4.0, 6.5, 9.0, 15.0, 0.2, 7.0, new Kind[1] { Kind.SoftFlake })
	};

	private static readonly LayerSpec[] HalloweenLayers = new LayerSpec[3]
	{
		new LayerSpec(11, 14.0, 19.0, 22.0, 34.0, 0.95, 26.0, new Kind[5]
		{
			Kind.Bat,
			Kind.Bat,
			Kind.Ghost,
			Kind.Candy,
			Kind.CandyCorn
		}),
		new LayerSpec(15, 8.5, 12.0, 14.0, 22.0, 0.62, 15.0, new Kind[4]
		{
			Kind.Bat,
			Kind.Ghost,
			Kind.Candy,
			Kind.CandyCorn
		}),
		new LayerSpec(30, 1.2, 2.6, 8.0, 16.0, 0.55, 7.0, new Kind[1] { Kind.Ember })
	};

	private readonly Random _random = new Random();

	private Scene _scene;

	private Scene? _previous;

	private DateTime _fadeStarted;

	private static readonly TimeSpan FadeDuration = TimeSpan.FromMilliseconds(1200.0);

	private static readonly TimeSpan FrameInterval = TimeSpan.FromMilliseconds(33.0);

	private DateTime _lastFrame = DateTime.UtcNow;

	private DateTime _lastDraw = DateTime.MinValue;

	private double _parallaxX;

	private double _parallaxY;

	private bool _running;

	private bool _paused;

	public static readonly DependencyProperty IsSnowEnabledProperty = DependencyProperty.Register("IsSnowEnabled", typeof(bool), typeof(SnowBackground), new PropertyMetadata(true, (DependencyObject d, DependencyPropertyChangedEventArgs _) =>
	{
		((SnowBackground)d).ApplyEnabled();
	}));

	private bool ParticlesOn
	{
		get
		{
			if (IsSnowEnabled)
			{
				return RenderBudget.AllowsAnimations;
			}
			return false;
		}
	}

	public bool IsSnowEnabled
	{
		get
		{
			return (bool)GetValue(IsSnowEnabledProperty);
		}
		set
		{
			SetValue(IsSnowEnabledProperty, value);
		}
	}

	public SnowBackground()
	{
		IsHitTestVisible = false;
		_scene = CreateScene();
		Loaded += (object _, RoutedEventArgs _) =>
		{
			Start();
		};
		Unloaded += (object _, RoutedEventArgs _) =>
		{
			Stop();
		};
		SizeChanged += (object _, SizeChangedEventArgs _) =>
		{
			Populate(_scene);
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
		RenderBudget.Changed += (object? _, EventArgs _) =>
		{
			ApplyEnabled();
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

	public void SetParallax(double normalizedX, double normalizedY)
	{
		_parallaxX = normalizedX;
		_parallaxY = normalizedY;
	}

	public void SwitchTheme()
	{
		_previous = (_running ? _scene : null);
		_scene = CreateScene();
		Populate(_scene);
		_fadeStarted = DateTime.UtcNow;
		InvalidateVisual();
	}

	private Scene CreateScene()
	{
		bool isPumpkin = ThemeManager.IsPumpkin;
		return new Scene
		{
			Pumpkin = isPumpkin,
			Backdrop = Frozen(new LinearGradientBrush(ThemeManager.C("Bg1"), ThemeManager.C("Bg0"), new Point(0.0, 0.0), new Point(0.0, 1.0))),
			GlowLeft = Glow(ThemeManager.C("AccentDim"), (byte)(isPumpkin ? 52 : 40)),
			GlowRight = Glow(ThemeManager.C("ArcEnd"), (byte)(isPumpkin ? 36 : 28))
		};
	}

	private static Brush Glow(Color color, byte alpha)
	{
		return Frozen(new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop(Color.FromArgb(alpha, color.R, color.G, color.B), 0.0),
				new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1.0)
			}
		});
	}

	private static T Frozen<T>(T freezable) where T : Freezable
	{
		freezable.Freeze();
		return freezable;
	}

	private void ApplyEnabled()
	{
		if (ParticlesOn)
		{
			Start();
		}
		else
		{
			Stop();
		}
		InvalidateVisual();
	}

	private void Start()
	{
		if (!_running && !_paused && ParticlesOn && IsLoaded && IsVisible)
		{
			Populate(_scene);
			_lastFrame = DateTime.UtcNow;
			CompositionTarget.Rendering += OnRendering;
			_running = true;
		}
	}

	private void Stop()
	{
		if (_running)
		{
			CompositionTarget.Rendering -= OnRendering;
			_running = false;
			_previous = null;
			InvalidateVisual();
		}
	}

	private void Populate(Scene scene)
	{
		if (ActualWidth <= 0.0 || ActualHeight <= 0.0)
		{
			return;
		}
		scene.Particles.Clear();
		LayerSpec[] array = (scene.Pumpkin ? HalloweenLayers : SnowLayers);
		foreach (LayerSpec layerSpec in array)
		{
			for (int j = 0; j < layerSpec.Count; j++)
			{
				scene.Particles.Add(new Particle
				{
					Layer = layerSpec,
					Kind = layerSpec.Kinds[_random.Next(layerSpec.Kinds.Length)],
					Size = Lerp(layerSpec.MinSize, layerSpec.MaxSize, _random.NextDouble()),
					X = _random.NextDouble() * ActualWidth,
					Y = _random.NextDouble() * ActualHeight,
					FallSpeed = Lerp(layerSpec.MinSpeed, layerSpec.MaxSpeed, _random.NextDouble()),
					DriftAmplitude = Lerp(6.0, 22.0, _random.NextDouble()),
					DriftFrequency = Lerp(0.25, 0.8, _random.NextDouble()),
					Phase = _random.NextDouble() * Math.PI * 2.0,
					Spin = Lerp(-40.0, 40.0, _random.NextDouble())
				});
			}
		}
	}

	private void OnRendering(object? sender, EventArgs e)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (utcNow - _lastDraw < FrameInterval)
		{
			return;
		}
		_lastDraw = utcNow;
		double num = (utcNow - _lastFrame).TotalSeconds;
		_lastFrame = utcNow;
		if ((num <= 0.0 || num > 0.2) ? true : false)
		{
			num = 1.0 / 30.0;
		}
		Advance(_scene, num);
		if (_previous != null)
		{
			Advance(_previous, num);
			if (utcNow - _fadeStarted >= FadeDuration)
			{
				_previous = null;
			}
		}
		InvalidateVisual();
	}

	private void Advance(Scene scene, double delta)
	{
		foreach (Particle particle in scene.Particles)
		{
			particle.Y += particle.FallSpeed * delta;
			if (particle.Y - particle.Size * 2.0 > ActualHeight)
			{
				particle.Y = (0.0 - particle.Size) * 3.0;
				particle.X = _random.NextDouble() * ActualWidth;
			}
		}
	}

	protected override void OnRender(DrawingContext dc)
	{
		double actualWidth = ActualWidth;
		double actualHeight = ActualHeight;
		if (!(actualWidth <= 0.0) && !(actualHeight <= 0.0))
		{
			double num = ((_previous == null) ? 1.0 : Math.Clamp((DateTime.UtcNow - _fadeStarted) / FadeDuration, 0.0, 1.0));
			num = num * num * (3.0 - 2.0 * num);
			if (_previous != null)
			{
				DrawScene(dc, _previous, actualWidth, actualHeight, 1.0);
				dc.PushOpacity(num);
				DrawScene(dc, _scene, actualWidth, actualHeight, 1.0);
				dc.Pop();
			}
			else
			{
				DrawScene(dc, _scene, actualWidth, actualHeight, 1.0);
			}
		}
	}

	private void DrawScene(DrawingContext dc, Scene scene, double width, double height, double opacity)
	{
		dc.DrawRectangle(scene.Backdrop, null, new Rect(0.0, 0.0, width, height));
		dc.DrawEllipse(scene.GlowLeft, null, new Point(width * 0.15, -60.0), 520.0, 340.0);
		dc.DrawEllipse(scene.GlowRight, null, new Point(width * 1.02, height * 0.2), 420.0, 320.0);
		if (!ParticlesOn)
		{
			return;
		}
		double num = (double)DateTime.UtcNow.Ticks / 10000000.0;
		foreach (Particle particle in scene.Particles)
		{
			double num2 = Math.Sin(num * particle.DriftFrequency + particle.Phase) * particle.DriftAmplitude;
			double num3 = particle.X + num2 + _parallaxX * particle.Layer.Parallax;
			double y = particle.Y + _parallaxY * particle.Layer.Parallax * 0.4;
			double num4 = particle.Size * 2.0;
			if (num3 < 0.0 - num4)
			{
				num3 += width + num4 * 2.0;
			}
			else if (num3 > width + num4)
			{
				num3 -= width + num4 * 2.0;
			}
			dc.PushOpacity(particle.Layer.Opacity * opacity);
			DrawParticle(dc, particle, num3, y, num);
			dc.Pop();
		}
	}

	private static void DrawParticle(DrawingContext dc, Particle p, double x, double y, double elapsed)
	{
		switch (p.Kind)
		{
		case Kind.Flake:
			dc.DrawEllipse(Sprites.Flake, null, new Point(x, y), p.Size, p.Size);
			return;
		case Kind.SoftFlake:
			dc.DrawEllipse(Sprites.SoftFlake, null, new Point(x, y), p.Size, p.Size);
			return;
		case Kind.Ember:
		{
			double opacity = 0.55 + 0.45 * Math.Sin(elapsed * 3.1 + p.Phase);
			dc.PushOpacity(opacity);
			dc.DrawEllipse(Sprites.Ember, null, new Point(x, y), p.Size * 1.8, p.Size * 1.8);
			dc.Pop();
			return;
		}
		}
		double num = p.Size / 12.0;
		Matrix identity = Matrix.Identity;
		identity.Translate(-12.0, -12.0);
		switch (p.Kind)
		{
		case Kind.Bat:
		{
			double num2 = 0.45 + 0.55 * Math.Abs(Math.Sin(elapsed * 9.0 + p.Phase));
			identity.Scale(num, num * num2);
			identity.Rotate(Math.Sin(elapsed * 1.3 + p.Phase) * 12.0);
			break;
		}
		case Kind.Ghost:
			identity.Scale(num, num);
			identity.Rotate(Math.Sin(elapsed * 1.1 + p.Phase) * 10.0);
			dc.PushOpacity(0.7 + 0.3 * Math.Sin(elapsed * 1.7 + p.Phase));
			break;
		default:
			identity.Scale(num, num);
			identity.Rotate(elapsed * p.Spin + p.Phase * 57.0);
			break;
		}
		identity.Translate(x, y);
		dc.PushTransform(new MatrixTransform(identity));
		dc.DrawDrawing(p.Kind switch
		{
			Kind.Bat => Sprites.Bat, 
			Kind.Ghost => Sprites.Ghost, 
			Kind.Candy => Sprites.Candy, 
			_ => Sprites.CandyCorn, 
		});
		dc.Pop();
		if (p.Kind == Kind.Ghost)
		{
			dc.Pop();
		}
	}

	private static double Lerp(double from, double to, double t)
	{
		return from + (to - from) * t;
	}
}
