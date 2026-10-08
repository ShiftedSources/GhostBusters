using System;
using System.Globalization;
using System.Windows.Data;

namespace GhostEye.App.Converters;

public sealed class ArrowIfTrueConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (!(value is bool) || !(bool)value)
		{
			return string.Empty;
		}
		return "  →  ";
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
