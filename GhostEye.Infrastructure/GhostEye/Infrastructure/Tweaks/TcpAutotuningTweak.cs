using System;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class TcpAutotuningTweak(IProcessRunner processRunner, ILocalizer loc) : ITweakAction
{
	private const string CorrectLevel = "normal";

	public string Id => "network.tcp-optimize";

	public async Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return await ReadLevelAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
	}

	public async Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		return string.Equals(await ReadLevelAsync(ct).ConfigureAwait(continueOnCapturedContext: false), "normal", StringComparison.OrdinalIgnoreCase);
	}

	public async Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		string text = await ReadLevelAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		if (text == null)
		{
			return TweakApplicability.No(loc["result.tcp.readFailed"]);
		}
		return string.Equals(text, "normal", StringComparison.OrdinalIgnoreCase) ? TweakApplicability.No(loc["applicability.tcpAlreadyNormal"]) : TweakApplicability.Applicable;
	}

	public async Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		string level = await ReadLevelAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		if (level == null)
		{
			return TweakResult.Fail(Id, loc["result.tcp.readFailed"]);
		}
		if (string.Equals(level, "normal", StringComparison.OrdinalIgnoreCase))
		{
			return TweakResult.Fail(Id, loc["result.tcp.alreadyNormal"]);
		}
		return (await processRunner.RunAsync("netsh.exe", "int tcp set global autotuninglevel=normal", ct).ConfigureAwait(continueOnCapturedContext: false)).Success ? TweakResult.Ok(Id, level, loc.Format("result.tcp.restored", level)) : TweakResult.Fail(Id, loc["result.tcp.failed"]);
	}

	public async Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		if (string.IsNullOrWhiteSpace(previousValue))
		{
			return TweakResult.Fail(Id, loc["result.tcp.noPrevious"]);
		}
		return (await processRunner.RunAsync("netsh.exe", "int tcp set global autotuninglevel=" + previousValue, ct).ConfigureAwait(continueOnCapturedContext: false)).Success ? TweakResult.Ok(Id, previousValue, loc.Format("result.tcp.revertedTo", previousValue)) : TweakResult.Fail(Id, loc["result.tcp.revertFailed"]);
	}

	private async Task<string?> ReadLevelAsync(CancellationToken ct)
	{
		ProcessResult processResult = await processRunner.RunAsync("netsh.exe", "int tcp show global", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!processResult.Success)
		{
			return null;
		}
		Match match = AutoTuningValue().Match(processResult.StandardOutput);
		return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex AutoTuningValue()
	{
		return _003CRegexGenerator_g_003EF0CC283204A2DE42F9EEE2FE380216117E81449C73179AF59228D883699E95A73__AutoTuningValue_9.Instance;
	}
}
