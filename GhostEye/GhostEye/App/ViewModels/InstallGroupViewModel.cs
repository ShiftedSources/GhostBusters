using System.Collections.Generic;
using GhostEye.App.Services;

namespace GhostEye.App.ViewModels;

public sealed class InstallGroupViewModel(string key, IReadOnlyList<InstallableViewModel> items)
{
	public string Key { get; } = key;

	public string Title => AppServices.Localizer["software.group." + Key];

	public IReadOnlyList<InstallableViewModel> Items { get; } = items;
}
