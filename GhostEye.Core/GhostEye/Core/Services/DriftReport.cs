using System;
using System.Collections.Generic;

namespace GhostEye.Core.Services;

public sealed record DriftReport(IReadOnlyList<string> ActiveTweakIds, IReadOnlyList<string> DriftedTweakIds)
{
	public static DriftReport Empty { get; } = new DriftReport(Array.Empty<string>(), Array.Empty<string>());

	public bool HasDrift => DriftedTweakIds.Count > 0;
}
