using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class StartupCleanupTweak(IStartupManagerService startup, ILocalizer loc) : ITweakAction
{
	private sealed record MovedEntry(string Name, string? Scope = null, string? Source = null);

	private static readonly string[] NonEssentialMarkers = new string[19]
	{
		"update", "updater", "helper", "assistant", "notifier", "spotify", "steam", "epicgames", "discord", "skype",
		"teams", "adobe", "acrotray", "ccleaner", "itunes", "quicktime", "java", "jusched", "cortana"
	};

	public string Id => "perf.startup-cleanup";

	public Task<string?> ReadCurrentValueAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(FindCandidates().Count.ToString());
	}

	public Task<bool> IsAppliedAsync(CancellationToken ct = default(CancellationToken))
	{
		List<StartupEntry> list = startup.GetEntries().Where(IsNonEssential).ToList();
		return Task.FromResult(list.Count > 0 && list.All((StartupEntry e) => e.IsDisabled));
	}

	public Task<TweakApplicability> GetApplicabilityAsync(CancellationToken ct = default(CancellationToken))
	{
		return Task.FromResult(startup.GetEntries().Any(IsNonEssential) ? TweakApplicability.Applicable : TweakApplicability.No(loc["applicability.startupNone"]));
	}

	public async Task<TweakResult> ApplyAsync(CancellationToken ct = default(CancellationToken))
	{
		List<StartupEntry> list = FindCandidates();
		if (list.Count == 0)
		{
			return TweakResult.Fail(Id, loc["applicability.startupNone"]);
		}
		List<MovedEntry> disabled = new List<MovedEntry>();
		List<string> failures = new List<string>();
		foreach (StartupEntry entry in list)
		{
			StartupOperationResult startupOperationResult = await startup.SetEnabledAsync(entry, enabled: false, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (startupOperationResult.Success)
			{
				disabled.Add(new MovedEntry(entry.Name, null, entry.Source.ToString()));
			}
			else
			{
				failures.Add(entry.Name + ": " + startupOperationResult.Error);
			}
		}
		if (disabled.Count == 0)
		{
			return TweakResult.Fail(Id, string.Join(" · ", failures));
		}
		string message = ((failures.Count == 0) ? loc.Format("result.startup.moved", disabled.Count) : (loc.Format("result.startup.movedPartial", disabled.Count, failures.Count) + " " + string.Join(" · ", failures)));
		return TweakResult.Ok(Id, JsonSerializer.Serialize(disabled), message);
	}

	public async Task<TweakResult> RevertAsync(string? previousValue, CancellationToken ct = default(CancellationToken))
	{
		List<MovedEntry> list;
		try
		{
			list = ((previousValue == null) ? null : JsonSerializer.Deserialize<List<MovedEntry>>(previousValue));
		}
		catch (JsonException)
		{
			list = null;
		}
		if (list == null || list.Count == 0)
		{
			return TweakResult.Fail(Id, loc["result.startup.noList"]);
		}
		IReadOnlyList<StartupEntry> current = startup.GetEntries();
		int restored = 0;
		List<string> failures = new List<string>();
		foreach (MovedEntry record in list)
		{
			if (record.Scope != null && Enum.TryParse<StartupScope>(record.Scope, out var result))
			{
				if (startup.TryRestore(record.Name, result, out string error))
				{
					restored++;
				}
				else
				{
					failures.Add(record.Name + ": " + error);
				}
				continue;
			}
			if (!Enum.TryParse<StartupSource>(record.Source, out var source))
			{
				failures.Add(record.Name);
				continue;
			}
			StartupEntry startupEntry = current.FirstOrDefault((StartupEntry e) => e.Source == source && e.Name.Equals(record.Name, StringComparison.OrdinalIgnoreCase));
			if ((object)startupEntry != null)
			{
				StartupOperationResult startupOperationResult = await startup.SetEnabledAsync(startupEntry, enabled: true, ct).ConfigureAwait(continueOnCapturedContext: false);
				if (startupOperationResult.Success)
				{
					restored++;
				}
				else
				{
					failures.Add(record.Name + ": " + startupOperationResult.Error);
				}
			}
		}
		return (restored == 0 && failures.Count > 0) ? TweakResult.Fail(Id, string.Join(" · ", failures)) : TweakResult.Ok(Id, previousValue, loc.Format("result.startup.restored", restored));
	}

	private List<StartupEntry> FindCandidates()
	{
		return (from e in startup.GetEntries()
			where e.IsEnabled && !e.IsRemoved
			select e).Where(IsNonEssential).ToList();
	}

	private static bool IsNonEssential(StartupEntry e)
	{
		if (!e.IsRemoved)
		{
			return NonEssentialMarkers.Any((string marker) => e.Name.Contains(marker, StringComparison.OrdinalIgnoreCase) || e.Command.Contains(marker, StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}
}
