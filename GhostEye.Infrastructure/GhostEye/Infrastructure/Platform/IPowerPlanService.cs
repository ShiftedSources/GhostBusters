using System;
using System.Collections.Generic;

namespace GhostEye.Infrastructure.Platform;

public interface IPowerPlanService
{
	Guid HighPerformanceGuid { get; }

	Guid? GetActiveScheme();

	bool TrySetActiveScheme(Guid schemeGuid, out string error);

	IReadOnlyList<PowerPlan> GetSchemes();

	string? GetSchemeName(Guid schemeGuid);

	bool IsHighPerformanceAvailable();
}
