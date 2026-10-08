using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.Services;

public static class TrayIcon
{
	private static NotifyIcon? _icon;

	private static string? _pendingPage;

	private static Image? _logo;

	public static void Initialize()
	{
		if (_icon != null)
		{
			return;
		}
		Icon icon = null;
		try
		{
			string processPath = Environment.ProcessPath;
			if (processPath != null)
			{
				icon = Icon.ExtractAssociatedIcon(processPath);
			}
		}
		catch (Exception ex) when ((ex is ArgumentException || ex is IOException) ? true : false)
		{
		}
		_icon = new NotifyIcon
		{
			Icon = (icon ?? SystemIcons.Application),
			Text = "GhostEye",
			ContextMenuStrip = new ContextMenuStrip(),
			Visible = true
		};
		_icon.ContextMenuStrip.Opening += (object? _, CancelEventArgs _) =>
		{
			BuildMenu();
		};
		_icon.ContextMenuStrip.Opened += (object? _, EventArgs _) =>
		{
			TrayMenuRenderer.RoundCorners(_icon.ContextMenuStrip.Handle);
		};
		_icon.MouseClick += (object? _, MouseEventArgs e) =>
		{
			if (e.Button == MouseButtons.Left)
			{
				App.ShowMainWindow();
			}
		};
		_icon.BalloonTipClicked += (object? _, EventArgs _) =>
		{
			App.ShowMainWindow();
			string pendingPage = _pendingPage;
			if (pendingPage != null)
			{
				AppServices.Shell?.NavigateTo(pendingPage);
			}
		};
		BuildMenu();
	}

	public static void Notify(string title, string text, string? openPage = null)
	{
		if (_icon != null && AppServices.Settings.Current.NotificationsEnabled)
		{
			_pendingPage = openPage;
			_icon.ShowBalloonTip(8000, title, text, ToolTipIcon.None);
		}
	}

	public static void SetStatus(string? status)
	{
		if (_icon != null)
		{
			string text = (string.IsNullOrEmpty(status) ? "GhostEye" : ("GhostEye · " + status));
			_icon.Text = ((text.Length > 63) ? text.Substring(0, 63) : text);
		}
	}

	public static void Dispose()
	{
		if (_icon != null)
		{
			_icon.Visible = false;
			_icon.Dispose();
			_icon = null;
		}
	}

	private static void BuildMenu()
	{
		ContextMenuStrip menu = _icon?.ContextMenuStrip;
		if (menu == null)
		{
			return;
		}
		ILocalizer localizer = AppServices.Localizer;
		menu.SuspendLayout();
		foreach (ToolStripItem item in menu.Items.OfType<ToolStripItem>().ToList())
		{
			menu.Items.Remove(item);
			if (!(item is TrayHeaderItem))
			{
				item.Dispose();
			}
		}
		if (!(menu.Renderer is TrayMenuRenderer))
		{
			menu.Renderer = new TrayMenuRenderer();
		}
		menu.BackColor = TrayMenuRenderer.Panel;
		menu.ForeColor = TrayMenuRenderer.Text;
		menu.Font = TrayMenuRenderer.ItemFont;
		menu.Padding = new Padding(0, Px(6.0), 0, Px(6.0));
		menu.ShowImageMargin = false;
		menu.ShowCheckMargin = false;
		BoostState state = AppServices.GameBoost.State;
		object obj;
		if ((object)state != null)
		{
			string autoGameName = state.AutoGameName;
			obj = ((autoGameName != null) ? localizer.Format("tray.status.boostFor", autoGameName) : localizer["tray.status.boost"]);
		}
		else
		{
			obj = null;
		}
		string text = (string)obj;
		if (_logo == null)
		{
			_logo = LoadLogo(Px(20.0));
		}
		List<ToolStripItem> list = new List<ToolStripItem>
		{
			new TrayHeaderItem(_logo, text),
			new ToolStripSeparator(),
			Item(localizer["tray.open"], "Icon.Dashboard", () =>
			{
				App.ShowMainWindow();
			})
		};
		bool isActive = AppServices.GameBoost.IsActive;
		list.Add(Item(localizer[isActive ? "tray.boost.stop" : "tray.boost.start"], "Icon.Gaming", async () =>
		{
			await BackgroundAgent.ToggleBoostFromTrayAsync();
		}, exit: false, isActive));
		list.Add(Item(localizer["tray.freeMemory"], "Icon.Tools", async () =>
		{
			await BackgroundAgent.FreeMemoryFromTrayAsync();
		}));
		list.Add(Item(localizer["tray.checkDrift"], "Icon.History", async () =>
		{
			await BackgroundAgent.CheckDriftAsync(notifyWhenClean: true);
		}));
		list.Add(Item(localizer["tray.maintenance"], "Icon.Cleanup", async () =>
		{
			await BackgroundAgent.RunMaintenanceFromTrayAsync();
		}));
		list.Add(new ToolStripSeparator());
		list.Add(Item(localizer["tray.exit"], "Icon.Close", App.RequestExit, exit: true));
		int num = (from i in list.OfType<TrayMenuItem>()
			select TextRenderer.MeasureText(i.Text, TrayMenuRenderer.ItemFont).Width).Append(TextRenderer.MeasureText("GhostEye", TrayMenuRenderer.HeaderFont).Width).Append((text != null) ? TextRenderer.MeasureText(text, TrayMenuRenderer.StatusFont).Width : 0).Max();
		int num2 = Math.Max(Px(220.0), Px(44.0) + num + Px(24.0));
		foreach (ToolStripItem item2 in list)
		{
			item2.AutoSize = false;
			item2.Margin = Padding.Empty;
			item2.Padding = Padding.Empty;
			int width = num2;
			int height;
			if (item2 is TrayHeaderItem)
			{
				height = Px((text == null) ? 44 : 54);
			}
			else
			{
				height = ((!(item2 is ToolStripSeparator)) ? Px(32.0) : Px(9.0));
			}
			item2.Size = new Size(width, height);
			menu.Items.Add(item2);
		}
		menu.ResumeLayout();
		TrayMenuItem Item(string text2, string icon, Action onClick, bool exit = false, bool accent = false)
		{
			Color color;
			if (exit)
			{
				color = TrayMenuRenderer.Danger;
			}
			else
			{
				color = (accent ? TrayMenuRenderer.Accent : TrayMenuRenderer.TextDim);
			}
			TrayMenuItem trayMenuItem = new TrayMenuItem(text2, TrayMenuRenderer.Icon(icon, color, Px(16.0)), exit);
			trayMenuItem.Click += (object? _, EventArgs _) =>
			{
				onClick();
			};
			return trayMenuItem;
		}
		int Px(double value)
		{
			return TrayMenuRenderer.Px(menu, value);
		}
	}

	private static Image? LoadLogo(int size)
	{
		try
		{
			string processPath = Environment.ProcessPath;
			object result;
			if (processPath != null)
			{
				Icon icon = Icon.ExtractAssociatedIcon(processPath);
				if (icon != null)
				{
					result = new Bitmap(icon.ToBitmap(), size, size);
					goto IL_0023;
				}
			}
			result = null;
			goto IL_0023;
			IL_0023:
			return (Image?)result;
		}
		catch (Exception ex) when ((ex is ArgumentException || ex is IOException) ? true : false)
		{
			return null;
		}
	}
}
