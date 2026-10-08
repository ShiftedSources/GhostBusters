using System;
using System.Globalization;
using System.Windows.Data;

namespace GhostEye.App.Converters;

public sealed class InverseBoolConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return !(value is bool) || !(bool)value;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		return !(value is bool) || !(bool)value;
	}
}
