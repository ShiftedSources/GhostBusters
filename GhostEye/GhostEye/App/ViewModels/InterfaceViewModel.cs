using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class InterfaceViewModel : CategoryPageViewModel
{
	public InterfaceViewModel()
		: base(TweakCategory.Interface, "interface.category", "Icon.Interface", "history.title.interface")
	{
	}
}
