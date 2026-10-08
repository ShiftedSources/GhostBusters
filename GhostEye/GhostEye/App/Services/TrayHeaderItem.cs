using System.Drawing;
using System.Windows.Forms;

namespace GhostEye.App.Services;

internal sealed class TrayHeaderItem : ToolStripLabel
{
	public Image? Logo { get; }

	public string? Status { get; }

	public TrayHeaderItem(Image? logo, string? status)
		: base("GhostEye")
	{
		Logo = logo;
		Status = status;
		AutoSize = false;
	}
}
