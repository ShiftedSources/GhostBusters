using System;

namespace GhostEye.Infrastructure.Health;

public sealed record DiskHealth
{
	public required string Name { get; init; }

	public string MediaType { get; init; } = string.Empty;

	public string BusType { get; init; } = string.Empty;

	public long SizeBytes { get; init; }

	public string HealthStatus { get; init; } = string.Empty;

	public int? TemperatureC { get; init; }

	public int? WearPercent { get; init; }

	public long? PowerOnHours { get; init; }

	public long? UncorrectedReadErrors { get; init; }

	public bool PredictsFailure { get; init; }

	public bool HasCounters
	{
		get
		{
			if (!TemperatureC.HasValue && !WearPercent.HasValue)
			{
				return PowerOnHours.HasValue;
			}
			return true;
		}
	}

	public HealthLevel Level
	{
		get
		{
			if (HealthStatus.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase) || PredictsFailure || UncorrectedReadErrors > 0 || WearPercent >= 90)
			{
				return HealthLevel.Bad;
			}
			if (HealthStatus.Equals("Warning", StringComparison.OrdinalIgnoreCase) || WearPercent >= 70 || TemperatureC >= 60)
			{
				return HealthLevel.Warning;
			}
			if (!HealthStatus.Equals("Healthy", StringComparison.OrdinalIgnoreCase))
			{
				return HealthLevel.Unknown;
			}
			return HealthLevel.Good;
		}
	}
}
