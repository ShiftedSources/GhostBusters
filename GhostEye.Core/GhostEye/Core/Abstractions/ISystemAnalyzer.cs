using System;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Models;

namespace GhostEye.Core.Abstractions;

public interface ISystemAnalyzer
{
	Task<AnalysisReport> AnalyzeAsync(IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken));
}
