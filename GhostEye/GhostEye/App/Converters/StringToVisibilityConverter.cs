using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GhostEye.App.Converters;

public sealed class StringToVisibilityConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return (string.IsNullOrWhiteSpace(value as string) ^ (parameter as string == "invert")) ? Visibility.Collapsed : Visibility.Visible;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
