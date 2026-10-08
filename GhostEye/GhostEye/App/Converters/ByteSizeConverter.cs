using System;
using System.Globalization;
using System.Windows.Data;
using GhostEye.App.Services;

namespace GhostEye.App.Converters;

public sealed class ByteSizeConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		long bytes;
		if (value is long num)
		{
			bytes = num;
		}
		else if (value is int num2)
		{
			bytes = num2;
		}
		else
		{
			bytes = ((!(value is double num3)) ? 0 : ((long)num3));
		}
		return GhostFormat.Bytes(bytes);
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
