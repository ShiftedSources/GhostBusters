using System.Drawing;
using System.Windows.Forms;

namespace GhostEye.App.Services;

internal sealed class TrayMenuItem : ToolStripMenuItem
{
	public Image? Glyph { get; }

	public bool IsExit { get; }

	public TrayMenuItem(string text, Image? glyph, bool isExit)
		: base(text)
	{
		Glyph = glyph;
		IsExit = isExit;
		AutoSize = false;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Glyph?.Dispose();
		}
		base.Dispose(disposing);
	}
}
