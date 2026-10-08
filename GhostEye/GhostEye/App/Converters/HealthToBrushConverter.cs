using System;
using System.Globalization;
using System.Windows.Data;
using GhostEye.Infrastructure.Health;

namespace GhostEye.App.Converters;

public sealed class HealthToBrushConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (!(value is HealthLevel))
		{
			goto IL_003b;
		}
		switch ((HealthLevel)value)
		{
		case HealthLevel.Good:
			break;
		case HealthLevel.Warning:
			goto IL_002b;
		case HealthLevel.Bad:
			goto IL_0033;
		default:
			goto IL_003b;
		}
		string key = "GoodBrush";
		goto IL_0041;
		IL_003b:
		key = "TextDimBrush";
		goto IL_0041;
		IL_0041:
		return Palette.Resolve(key);
		IL_002b:
		key = "WarnBrush";
		goto IL_0041;
		IL_0033:
		key = "DangerBrush";
		goto IL_0041;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
