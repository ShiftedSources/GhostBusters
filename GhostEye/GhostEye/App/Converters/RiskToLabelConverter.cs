using System;
using System.Globalization;
using System.Windows.Data;
using GhostEye.App.Services;
using GhostEye.Core.Models;

namespace GhostEye.App.Converters;

public sealed class RiskToLabelConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is RiskLevel)
		{
			switch ((RiskLevel)value)
			{
			case RiskLevel.Low:
				return AppServices.Localizer["risk.low"];
			case RiskLevel.Medium:
				return AppServices.Localizer["risk.med"];
			case RiskLevel.High:
				return AppServices.Localizer["risk.high"];
			}
		}
		return string.Empty;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
