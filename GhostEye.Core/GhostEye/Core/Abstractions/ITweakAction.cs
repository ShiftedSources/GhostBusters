using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Models;

namespace GhostEye.Core.Abstractions;

public interface ITweakAction
{
	string Id { get; }

	Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken));

	Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken));

	Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken));

	Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken));

	Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken));
}
