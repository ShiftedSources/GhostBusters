using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GhostEye.App.Converters;

public sealed class RatioToStarConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		double num = ((value is double num2) ? num2 : 0.0);
		string obj = (parameter as string) ?? string.Empty;
		if (obj.Contains("percent", StringComparison.Ordinal))
		{
			num /= 100.0;
		}
		num = Math.Clamp(num, 0.0, 1.0);
		if (obj.Contains("rest", StringComparison.Ordinal))
		{
			num = 1.0 - num;
		}
		return new GridLength(Math.Max(num, 0.0001), GridUnitType.Star);
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
