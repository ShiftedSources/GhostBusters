using CommunityToolkit.Mvvm.ComponentModel;
using GhostEye.App.Services;
using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class PresetViewModel(Preset preset) : ObservableObject, ILocalizedViewModel
{
	public Preset Preset { get; } = preset;

	public string Id => Preset.Id;

	public string AccentColor => Preset.AccentColor;

	public string IconKey => Preset.IconKey;

	public string Name => AppServices.Localizer[Preset.NameKey];

	public string Description => AppServices.Localizer[Preset.DescriptionKey];

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Name");
		OnPropertyChanged("Description");
	}
}
