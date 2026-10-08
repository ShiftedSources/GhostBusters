using System;
using System.Globalization;
using System.Windows.Data;
using GhostEye.Core.Models;

namespace GhostEye.App.Converters;

public sealed class RiskToBrushConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (!(value is RiskLevel))
		{
			goto IL_003b;
		}
		switch ((RiskLevel)value)
		{
		case RiskLevel.Low:
			break;
		case RiskLevel.Medium:
			goto IL_002b;
		case RiskLevel.High:
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
