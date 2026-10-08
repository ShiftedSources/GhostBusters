using System.Windows;
using System.Windows.Media;

namespace GhostEye.App.Converters;

internal static class Palette
{
	public static Brush Resolve(string key)
	{
		return (Application.Current?.TryFindResource(key) as Brush) ?? Brushes.Gray;
	}
}
