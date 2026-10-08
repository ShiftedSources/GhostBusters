using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class MemoryCompressionTweak(IProcessRunner processRunner, ILocalizer loc) : ITweakAction
{
	public string Id => "perf.memory-compression";

	public async Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return (await ReadStateAsync(ct).ConfigureAwait(continueOnCapturedContext: false))?.ToString();
	}

	public async Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		return await ReadStateAsync(ct).ConfigureAwait(continueOnCapturedContext: false) == false;
	}

	public async Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		return (!(await ReadStateAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).HasValue) ? TweakApplicability.No(loc["applicability.memoryUnreadable"]) : TweakApplicability.Applicable;
	}

	public async Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		bool? previous = await ReadStateAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!previous.HasValue)
		{
			return TweakResult.Fail(Id, loc["result.memory.readFailed"]);
		}
		ProcessResult processResult = await processRunner.RunPowerShellAsync("Disable-MMAgent -mc", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!processResult.Success)
		{
			return TweakResult.Fail(Id, loc.Format("result.memory.disableFailed", Summarize(processResult.Combined)));
		}
		return TweakResult.Ok(Id, previous.Value.ToString(), loc["result.memory.disabled"]);
	}

	public async Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		if (!bool.TryParse(previousValue, out var wasEnabled))
		{
			return TweakResult.Fail(Id, loc["result.memory.noPrevious"]);
		}
		string command = (wasEnabled ? "Enable-MMAgent -mc" : "Disable-MMAgent -mc");
		ProcessResult processResult = await processRunner.RunPowerShellAsync(command, ct).ConfigureAwait(continueOnCapturedContext: false);
		return processResult.Success ? TweakResult.Ok(Id, previousValue, wasEnabled ? loc["result.memory.reEnabled"] : loc["result.memory.leftDisabled"]) : TweakResult.Fail(Id, loc.Format("result.memory.revertFailed", Summarize(processResult.Combined)));
	}

	private async Task<bool?> ReadStateAsync(CancellationToken ct)
	{
		ProcessResult processResult = await processRunner.RunPowerShellAsync("(Get-MMAgent).MemoryCompression", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!processResult.Success)
		{
			return null;
		}
		bool result;
		return bool.TryParse(processResult.StandardOutput.Trim(), out result) ? new bool?(result) : ((bool?)null);
	}

	private string Summarize(string output)
	{
		string text = output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault((string l) => l.Length > 0);
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return loc["result.memory.noDetail"];
	}
}
