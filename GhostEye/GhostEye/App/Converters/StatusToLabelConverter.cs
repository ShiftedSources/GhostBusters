using System;
using System.Globalization;
using System.Windows.Data;
using GhostEye.App.Services;
using GhostEye.Core.Models;

namespace GhostEye.App.Converters;

public sealed class StatusToLabelConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is AnalysisStatus)
		{
			switch ((AnalysisStatus)value)
			{
			case AnalysisStatus.Ok:
				return AppServices.Localizer["status.ok"];
			case AnalysisStatus.Warning:
				return AppServices.Localizer["status.warning"];
			case AnalysisStatus.Critical:
				return AppServices.Localizer["status.critical"];
			}
		}
		return AppServices.Localizer["status.none"];
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotSupportedException();
	}
}
