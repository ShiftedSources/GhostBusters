using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostEye.Core.Models;

public sealed record AnalysisReport
{
	public required IReadOnlyList<AnalysisItem> Items { get; init; }

	public required int Score { get; init; }

	public required DateTimeOffset CompletedAt { get; init; }

	public IEnumerable<AnalysisItem> Warnings => Items.Where((AnalysisItem i) =>
	{
		AnalysisStatus status = i.Status;
		return (uint)(status - 1) <= 1u;
	});
}
