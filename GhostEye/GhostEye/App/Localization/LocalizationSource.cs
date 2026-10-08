using System.ComponentModel;
using GhostEye.App.Services;

namespace GhostEye.App.Localization;

public sealed class LocalizationSource : INotifyPropertyChanged
{
	public static LocalizationSource Instance { get; } = new LocalizationSource();

	public string this[string key] => AppServices.Localizer[key];

	public event PropertyChangedEventHandler? PropertyChanged;

	private LocalizationSource()
	{
	}

	public void Refresh()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
	}
}
