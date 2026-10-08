using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using GhostEye.Backup;

namespace GhostEye.App.Controls;

internal static class CoinRenderer
{
	public const double Thickness = 0.22;

	private const double CapGap = 0.002;

	private const int WallGrid = 384;

	private const double EdgeOnCosine = 0.12;

	private const int Supersampling = 4;

	private const int RenderVersion = 3;

	private const double CameraDistance = 3.2;

	private const double FieldOfView = 40.0;

	public static double FaceScale => 2.0 / (6.180000000000001 * Math.Tan(Math.PI / 9.0));

	public static BitmapSource[] LoadOrRenderTurn(int pixels, int frameCount, double dpi)
	{
		int num = pixels * pixels * 4;
		string text = null;
		try
		{
			text = Path.Combine(AppPaths.Cache, $"logo-{pixels}px-{CacheKey(pixels, frameCount)}.bin");
			if (File.Exists(text))
			{
				byte[] array = File.ReadAllBytes(text);
				if (array.Length == num * frameCount)
				{
					BitmapSource[] array2 = new BitmapSource[frameCount];
					for (int i = 0; i < frameCount; i++)
					{
						array2[i] = BitmapSource.Create(pixels, pixels, dpi, dpi, PixelFormats.Pbgra32, null, array.AsSpan(i * num, num).ToArray(), pixels * 4);
						array2[i].Freeze();
					}
					return array2;
				}
			}
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
		BitmapSource[] array3 = RenderTurn(pixels, frameCount, dpi);
		if (text != null)
		{
			try
			{
				byte[] array4 = new byte[num * frameCount];
				for (int j = 0; j < frameCount; j++)
				{
					array3[j].CopyPixels(array4, pixels * 4, j * num);
				}
				string text2 = text + ".tmp";
				File.WriteAllBytes(text2, array4);
				File.Move(text2, text, overwrite: true);
				foreach (string item in Directory.EnumerateFiles(Path.GetDirectoryName(text), $"logo-{pixels}px-*.bin"))
				{
					if (!string.Equals(item, text, StringComparison.OrdinalIgnoreCase))
					{
						File.Delete(item);
					}
				}
			}
			catch (Exception ex2) when ((ex2 is IOException || ex2 is UnauthorizedAccessException) ? true : false)
			{
			}
		}
		return array3;
	}

	private static string CacheKey(int pixels, int frameCount)
	{
		using SHA256 sHA = SHA256.Create();
		string[] array = new string[2] { "logo.png", "logo-edge.png" };
		foreach (string text in array)
		{
			using Stream stream = Application.GetResourceStream(new Uri("pack://application:,,,/Assets/" + text)).Stream;
			byte[] array2 = new byte[stream.Length];
			stream.ReadExactly(array2);
			sHA.TransformBlock(array2, 0, array2.Length, null, 0);
		}
		byte[] bytes = Encoding.UTF8.GetBytes(string.Join('|', 3, pixels, frameCount, 0.22, 0.002, 384, 0.12, 4, 3.2, 40.0));
		sHA.TransformFinalBlock(bytes, 0, bytes.Length);
		return Convert.ToHexString(sHA.Hash).Substring(0, 16).ToLowerInvariant();
	}

	public static BitmapSource[] RenderTurn(int pixels, int frameCount, double dpi)
	{
		int num = pixels * 4;
		Material front = CreateMaterial("logo.png", mirrored: false);
		Material back = CreateMaterial("logo.png", mirrored: true);
		Material front2 = CreateMaterial("logo-edge.png", mirrored: false);
		Material back2 = CreateMaterial("logo-edge.png", mirrored: true);
		MeshGeometry3D quad = CreateQuad();
		GeometryModel3D value = CreateWalls();
		double num2 = 0.11;
		List<GeometryModel3D> list = new List<GeometryModel3D>
		{
			CreateLayer(quad, 0.0 - num2, null, back),
			CreateLayer(quad, 0.0 - num2 + 0.002, front2, back2),
			CreateLayer(quad, num2 - 0.002, front2, back2),
			CreateLayer(quad, num2, front, null)
		};
		AxisAngleRotation3D axisAngleRotation3D = new AxisAngleRotation3D(new Vector3D(0.0, 1.0, 0.0), 0.0);
		Model3DGroup model3DGroup = new Model3DGroup
		{
			Transform = new RotateTransform3D(axisAngleRotation3D)
		};
		Model3DGroup model3DGroup2 = new Model3DGroup();
		model3DGroup2.Children.Add(new AmbientLight(Colors.White));
		model3DGroup2.Children.Add(model3DGroup);
		Viewport3D viewport3D = new Viewport3D
		{
			Width = num,
			Height = num,
			ClipToBounds = false,
			Camera = new PerspectiveCamera(new Point3D(0.0, 0.0, 3.2), new Vector3D(0.0, 0.0, -1.0), new Vector3D(0.0, 1.0, 0.0), 40.0)
		};
		viewport3D.Children.Add(new ModelVisual3D
		{
			Content = model3DGroup2
		});
		viewport3D.Measure(new Size(num, num));
		viewport3D.Arrange(new Rect(0.0, 0.0, num, num));
		BitmapSource[] array = new BitmapSource[frameCount];
		for (int i = 0; i < frameCount; i++)
		{
			double num3 = (axisAngleRotation3D.Angle = 360.0 * (double)i / (double)frameCount);
			double num5 = Math.Cos(num3 * Math.PI / 180.0);
			model3DGroup.Children.Clear();
			model3DGroup.Children.Add(value);
			if (Math.Abs(num5) >= 0.12)
			{
				IEnumerable<GeometryModel3D> enumerable;
				if (!(num5 > 0.0))
				{
					enumerable = Enumerable.Reverse(list);
				}
				else
				{
					IEnumerable<GeometryModel3D> enumerable2 = list;
					enumerable = enumerable2;
				}
				foreach (GeometryModel3D item in enumerable)
				{
					model3DGroup.Children.Add(item);
				}
			}
			viewport3D.UpdateLayout();
			RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(num, num, 96.0, 96.0, PixelFormats.Pbgra32);
			renderTargetBitmap.Render(viewport3D);
			array[i] = Downscale(renderTargetBitmap, pixels, dpi);
		}
		return array;
	}

	private static BitmapSource Downscale(BitmapSource source, int pixels, double dpi)
	{
		int num = pixels * 4;
		byte[] array = new byte[num * num * 4];
		source.CopyPixels(array, num * 4, 0);
		byte[] array2 = new byte[pixels * pixels * 4];
		int num2 = 16;
		for (int i = 0; i < pixels; i++)
		{
			for (int j = 0; j < pixels; j++)
			{
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int num6 = 0;
				for (int k = 0; k < 4; k++)
				{
					int num7 = ((i * 4 + k) * num + j * 4) * 4;
					for (int l = 0; l < 4; l++)
					{
						int num8 = num7 + l * 4;
						num3 += array[num8];
						num4 += array[num8 + 1];
						num5 += array[num8 + 2];
						num6 += array[num8 + 3];
					}
				}
				int num9 = (i * pixels + j) * 4;
				array2[num9] = (byte)((num3 + num2 / 2) / num2);
				array2[num9 + 1] = (byte)((num4 + num2 / 2) / num2);
				array2[num9 + 2] = (byte)((num5 + num2 / 2) / num2);
				array2[num9 + 3] = (byte)((num6 + num2 / 2) / num2);
			}
		}
		BitmapSource bitmapSource = BitmapSource.Create(pixels, pixels, dpi, dpi, PixelFormats.Pbgra32, null, array2, pixels * 4);
		bitmapSource.Freeze();
		return bitmapSource;
	}

	private static GeometryModel3D CreateLayer(MeshGeometry3D quad, double z, Material? front, Material? back)
	{
		return new GeometryModel3D(quad, front)
		{
			BackMaterial = back,
			Transform = new TranslateTransform3D(0.0, 0.0, z)
		};
	}

	private static MeshGeometry3D CreateQuad()
	{
		return new MeshGeometry3D
		{
			Positions = new Point3DCollection
			{
				new Point3D(-1.0, -1.0, 0.0),
				new Point3D(1.0, -1.0, 0.0),
				new Point3D(1.0, 1.0, 0.0),
				new Point3D(-1.0, 1.0, 0.0)
			},
			TextureCoordinates = new PointCollection
			{
				new Point(0.0, 1.0),
				new Point(1.0, 1.0),
				new Point(1.0, 0.0),
				new Point(0.0, 0.0)
			},
			TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 }
		};
	}

	private static BitmapSource LoadAsset(string asset)
	{
		BitmapImage bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.UriSource = new Uri("pack://application:,,,/Assets/" + asset);
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		bitmapImage.EndInit();
		bitmapImage.Freeze();
		return bitmapImage;
	}

	private static Material CreateMaterial(string asset, bool mirrored)
	{
		ImageBrush imageBrush = new ImageBrush(LoadAsset(asset))
		{
			Stretch = Stretch.Uniform
		};
		RenderOptions.SetBitmapScalingMode(imageBrush, BitmapScalingMode.HighQuality);
		if (mirrored)
		{
			imageBrush.RelativeTransform = new ScaleTransform(-1.0, 1.0, 0.5, 0.5);
		}
		return new DiffuseMaterial(imageBrush);
	}

	private static GeometryModel3D CreateWalls()
	{
		FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(LoadAsset("logo-edge.png"), PixelFormats.Bgra32, null, 0.0);
		int width = formatConvertedBitmap.PixelWidth;
		int height = formatConvertedBitmap.PixelHeight;
		byte[] pixels = new byte[width * height * 4];
		formatConvertedBitmap.CopyPixels(pixels, width * 4, 0);
		double half = 0.11;
		MeshGeometry3D mesh = new MeshGeometry3D();
		for (int i = 0; i < 384; i++)
		{
			for (int j = 0; j < 384; j++)
			{
				int num = (Inside(j, i) ? 8 : 0) | (Inside(j + 1, i) ? 4 : 0) | (Inside(j + 1, i + 1) ? 2 : 0) | (Inside(j, i + 1) ? 1 : 0);
				double num2 = (double)j + 0.5;
				double num3 = i;
				double x = j + 1;
				double y = (double)i + 0.5;
				double num4 = (double)j + 0.5;
				double num5 = i + 1;
				double x2 = j;
				double y2 = (double)i + 0.5;
				switch (num)
				{
				case 1:
				case 14:
					AddWall(x2, y2, num4, num5);
					break;
				case 2:
				case 13:
					AddWall(num4, num5, x, y);
					break;
				case 3:
				case 12:
					AddWall(x2, y2, x, y);
					break;
				case 4:
				case 11:
					AddWall(num2, num3, x, y);
					break;
				case 6:
				case 9:
					AddWall(num2, num3, num4, num5);
					break;
				case 7:
				case 8:
					AddWall(x2, y2, num2, num3);
					break;
				case 5:
					AddWall(x2, y2, num2, num3);
					AddWall(num4, num5, x, y);
					break;
				case 10:
					AddWall(num2, num3, x, y);
					AddWall(x2, y2, num4, num5);
					break;
				}
			}
		}
		DiffuseMaterial diffuseMaterial = new DiffuseMaterial(new LinearGradientBrush
		{
			StartPoint = new Point(0.0, 0.0),
			EndPoint = new Point(0.0, 1.0),
			GradientStops = new GradientStopCollection
			{
				new GradientStop(Color.FromRgb(38, 35, 51), 0.0),
				new GradientStop(Color.FromRgb(70, 66, 86), 0.5),
				new GradientStop(Color.FromRgb(38, 35, 51), 1.0)
			}
		});
		return new GeometryModel3D(mesh, diffuseMaterial)
		{
			BackMaterial = diffuseMaterial
		};
		void AddWall(double num6, double num7, double num8, double num9)
		{
			double x3 = num6 / 384.0 * 2.0 - 1.0;
			double y3 = 1.0 - num7 / 384.0 * 2.0;
			double x4 = num8 / 384.0 * 2.0 - 1.0;
			double y4 = 1.0 - num9 / 384.0 * 2.0;
			int count = mesh.Positions.Count;
			mesh.Positions.Add(new Point3D(x3, y3, 0.0 - half));
			mesh.Positions.Add(new Point3D(x3, y3, half));
			mesh.Positions.Add(new Point3D(x4, y4, 0.0 - half));
			mesh.Positions.Add(new Point3D(x4, y4, half));
			mesh.TextureCoordinates.Add(new Point(0.0, 0.0));
			mesh.TextureCoordinates.Add(new Point(0.0, 1.0));
			mesh.TextureCoordinates.Add(new Point(1.0, 0.0));
			mesh.TextureCoordinates.Add(new Point(1.0, 1.0));
			mesh.TriangleIndices.Add(count);
			mesh.TriangleIndices.Add(count + 2);
			mesh.TriangleIndices.Add(count + 1);
			mesh.TriangleIndices.Add(count + 1);
			mesh.TriangleIndices.Add(count + 2);
			mesh.TriangleIndices.Add(count + 3);
		}
		bool Inside(int gx, int gy)
		{
			int num6 = Math.Min(width - 1, gx * width / 384);
			int num7 = Math.Min(height - 1, gy * height / 384);
			return pixels[(num7 * width + num6) * 4 + 3] >= 128;
		}
	}
}
