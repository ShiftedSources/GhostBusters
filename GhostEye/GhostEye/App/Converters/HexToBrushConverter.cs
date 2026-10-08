using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GhostEye.App.Converters;

public sealed class HexToBrushConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string value2 && !string.IsNullOrWhiteSpace(value2))
		{
			try
			{
				return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value2));
			}
			catch (FormatException)
			{
			}
		}
		return Palette.Resolve("TextDimBrush");
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
