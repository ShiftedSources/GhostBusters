using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class PrivacyViewModel : CategoryPageViewModel
{
	public PrivacyViewModel()
		: base(TweakCategory.Privacy, "privacy.category", "Icon.Privacy", "history.title.privacy")
	{
	}
}
