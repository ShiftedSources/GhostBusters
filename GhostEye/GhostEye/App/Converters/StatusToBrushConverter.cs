using System;
using System.Globalization;
using System.Windows.Data;
using GhostEye.Core.Models;

namespace GhostEye.App.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (!(value is AnalysisStatus))
		{
			goto IL_003b;
		}
		switch ((AnalysisStatus)value)
		{
		case AnalysisStatus.Ok:
			break;
		case AnalysisStatus.Warning:
			goto IL_002b;
		case AnalysisStatus.Critical:
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
