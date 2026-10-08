using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GhostEye.App.Services;

internal sealed class TrayMenuRenderer : ToolStripRenderer
{
	public static readonly System.Drawing.Color Danger = System.Drawing.Color.FromArgb(226, 104, 95);

	public static readonly System.Drawing.Color DangerWash = System.Drawing.Color.FromArgb(58, 26, 34);

	public const int Inset = 6;

	public const int IconLeft = 16;

	public const int IconSize = 16;

	public const int LogoSize = 20;

	public const int TextLeft = 44;

	public const int RightPadding = 24;

	public const int ItemHeight = 32;

	public const int HeaderHeight = 44;

	public const int HeaderWithStatusHeight = 54;

	public const int SeparatorHeight = 9;

	public static readonly Font ItemFont = new Font("Segoe UI", 9.75f);

	public static readonly Font HeaderFont = new Font("Segoe UI Semibold", 10.5f);

	public static readonly Font StatusFont = new Font("Segoe UI", 8.25f);

	private const TextFormatFlags LineFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding;

	public static System.Drawing.Color Panel => Of(ThemeManager.C("Panel"));

	public static System.Drawing.Color Line => Of(ThemeManager.C("Line"));

	public static System.Drawing.Color Hover => Mix(ThemeManager.C("PanelHover"), ThemeManager.C("Accent"), 0.12);

	public static System.Drawing.Color Accent => Of(ThemeManager.C("Accent"));

	public static System.Drawing.Color Text => Of(ThemeManager.C("Text"));

	public static System.Drawing.Color TextDim => Of(ThemeManager.C("TextDim"));

	private static System.Drawing.Color Of(System.Windows.Media.Color c)
	{
		return System.Drawing.Color.FromArgb(c.R, c.G, c.B);
	}

	private static System.Drawing.Color Mix(System.Windows.Media.Color a, System.Windows.Media.Color b, double t)
	{
		return System.Drawing.Color.FromArgb((int)((double)(int)a.R + (double)(b.R - a.R) * t), (int)((double)(int)a.G + (double)(b.G - a.G) * t), (int)((double)(int)a.B + (double)(b.B - a.B) * t));
	}

	public static int Px(ToolStrip? strip, double value)
	{
		return (int)Math.Round(value * (double)(strip?.DeviceDpi ?? 96) / 96.0);
	}

	protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
	{
		using SolidBrush brush = new SolidBrush(Panel);
		e.Graphics.FillRectangle(brush, e.AffectedBounds);
	}

	protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
	{
		using System.Drawing.Pen pen = new System.Drawing.Pen(Line);
		Rectangle affectedBounds = e.AffectedBounds;
		e.Graphics.DrawRectangle(pen, 0, 0, affectedBounds.Width - 1, affectedBounds.Height - 1);
	}

	protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
	{
	}

	protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
	{
		if (!(e.Item is TrayMenuItem { Selected: not false, Enabled: not false } trayMenuItem))
		{
			return;
		}
		ToolStrip toolStrip = e.ToolStrip;
		Rectangle r = new Rectangle(Px(toolStrip, 6.0), Px(toolStrip, 1.0), trayMenuItem.Width - 2 * Px(toolStrip, 6.0), trayMenuItem.Height - Px(toolStrip, 2.0));
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		using (GraphicsPath path = RoundedRect(r, Px(toolStrip, 6.0)))
		{
			using SolidBrush brush = new SolidBrush(trayMenuItem.IsExit ? DangerWash : Hover);
			e.Graphics.FillPath(brush, path);
		}
		int num = r.Height / 2;
		using GraphicsPath path2 = RoundedRect(new Rectangle(r.X, r.Y + (r.Height - num) / 2, Px(toolStrip, 3.0), num), Px(toolStrip, 1.0));
		using SolidBrush brush2 = new SolidBrush(trayMenuItem.IsExit ? Danger : Accent);
		e.Graphics.FillPath(brush2, path2);
	}

	protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
	{
	}

	protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
	{
		ToolStrip toolStrip = e.ToolStrip;
		Graphics graphics = e.Graphics;
		graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
		ToolStripItem item = e.Item;
		if (!(item is TrayMenuItem trayMenuItem))
		{
			if (item is TrayHeaderItem trayHeaderItem)
			{
				int num = Px(toolStrip, 20.0);
				int x = Px(toolStrip, 16.0) + (Px(toolStrip, 16.0) - num) / 2;
				Image logo = trayHeaderItem.Logo;
				if (logo != null)
				{
					graphics.DrawImage(logo, x, (trayHeaderItem.Height - num) / 2, num, num);
				}
				int width = trayHeaderItem.Width - Px(toolStrip, 44.0) - Px(toolStrip, 24.0);
				if (trayHeaderItem.Status == null)
				{
					TextRenderer.DrawText(graphics, trayHeaderItem.Text, HeaderFont, new Rectangle(Px(toolStrip, 44.0), 0, width, trayHeaderItem.Height), Text, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
					return;
				}
				int height = TextRenderer.MeasureText(graphics, trayHeaderItem.Text, HeaderFont, System.Drawing.Size.Empty, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding).Height;
				int height2 = TextRenderer.MeasureText(graphics, trayHeaderItem.Status, StatusFont, System.Drawing.Size.Empty, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding).Height;
				int num2 = (trayHeaderItem.Height - height - height2) / 2;
				TextRenderer.DrawText(graphics, trayHeaderItem.Text, HeaderFont, new Rectangle(Px(toolStrip, 44.0), num2, width, height), Text, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
				TextRenderer.DrawText(graphics, trayHeaderItem.Status, StatusFont, new Rectangle(Px(toolStrip, 44.0), num2 + height, width, height2), Accent, TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
			}
			else
			{
				base.OnRenderItemText(e);
			}
		}
		else
		{
			int num3 = Px(toolStrip, 16.0);
			Image glyph = trayMenuItem.Glyph;
			if (glyph != null)
			{
				graphics.DrawImage(glyph, Px(toolStrip, 16.0), (trayMenuItem.Height - num3) / 2, num3, num3);
			}
			System.Drawing.Color foreColor = ((trayMenuItem.IsExit && trayMenuItem.Selected) ? Danger : Text);
			TextRenderer.DrawText(bounds: new Rectangle(Px(toolStrip, 44.0), 0, trayMenuItem.Width - Px(toolStrip, 44.0) - Px(toolStrip, 24.0), trayMenuItem.Height), dc: graphics, text: trayMenuItem.Text, font: ItemFont, foreColor: foreColor, flags: TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
		}
	}

	protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
	{
		using System.Drawing.Pen pen = new System.Drawing.Pen(Line);
		ToolStrip toolStrip = e.ToolStrip;
		int num = e.Item.Height / 2;
		e.Graphics.DrawLine(pen, Px(toolStrip, 12.0), num, e.Item.Width - Px(toolStrip, 12.0), num);
	}

	private static GraphicsPath RoundedRect(Rectangle r, int radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		int num = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
		graphicsPath.AddArc(r.X, r.Y, num, num, 180f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Y, num, num, 270f, 90f);
		graphicsPath.AddArc(r.Right - num, r.Bottom - num, num, num, 0f, 90f);
		graphicsPath.AddArc(r.X, r.Bottom - num, num, num, 90f, 90f);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static Image? Icon(string resourceKey, System.Drawing.Color color, int size)
	{
		if (!(System.Windows.Application.Current?.TryFindResource(resourceKey) is Geometry geometry))
		{
			return null;
		}
		DrawingVisual drawingVisual = new DrawingVisual();
		using (DrawingContext drawingContext = drawingVisual.RenderOpen())
		{
			double num = (double)size / 24.0;
			drawingContext.PushTransform(new ScaleTransform(num, num));
			System.Windows.Media.Pen pen = new System.Windows.Media.Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B)), 1.8)
			{
				StartLineCap = PenLineCap.Round,
				EndLineCap = PenLineCap.Round,
				LineJoin = PenLineJoin.Round
			};
			drawingContext.DrawGeometry(null, pen, geometry);
		}
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(size, size, 96.0, 96.0, PixelFormats.Pbgra32);
		renderTargetBitmap.Render(drawingVisual);
		PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
		pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
		MemoryStream memoryStream = new MemoryStream();
		pngBitmapEncoder.Save(memoryStream);
		memoryStream.Position = 0L;
		return Image.FromStream(memoryStream);
	}

	public static void RoundCorners(nint handle)
	{
		int value = 2;
		DwmSetWindowAttribute(handle, 33, ref value, 4);
	}

	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);
}
