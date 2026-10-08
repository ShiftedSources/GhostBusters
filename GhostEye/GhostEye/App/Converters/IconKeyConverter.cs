using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GhostEye.App.Converters;

public sealed class IconKeyConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (!(value is string resourceKey))
		{
			return null;
		}
		return Application.Current?.TryFindResource(resourceKey) as Geometry;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
