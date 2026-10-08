using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;

namespace GhostEye.Core.Services;

public sealed class OptimizationEngine : IOptimizationEngine
{
	private readonly IReadOnlyDictionary<string, ITweakAction> _actions;

	private readonly IRestorePointService _restorePoints;

	private readonly IRegistryBackupService _registryBackup;

	private readonly IChangeHistoryStore _history;

	private readonly ILocalizer _loc;

	private readonly IAppLogger _logger;

	public IReadOnlyList<TweakDefinition> Catalog => TweakCatalog.All;

	public OptimizationEngine(IEnumerable<ITweakAction> actions, IRestorePointService restorePoints, IRegistryBackupService registryBackup, IChangeHistoryStore history, ILocalizer loc, IAppLogger logger)
	{
		_actions = actions.ToDictionary((ITweakAction a) => a.Id, StringComparer.Ordinal);
		_restorePoints = restorePoints;
		_registryBackup = registryBackup;
		_history = history;
		_loc = loc;
		_logger = logger;
	}

	public async Task<IReadOnlyDictionary<string, string?>> ReadCurrentValuesAsync(IEnumerable<string> tweakIds, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<string, string?> result = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (string id in tweakIds)
		{
			ct.ThrowIfCancellationRequested();
			if (_actions.TryGetValue(id, out ITweakAction value))
			{
				try
				{
					Dictionary<string, string?> dictionary = result;
					string key = id;
					dictionary[key] = await value.ReadCurrentValueAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception exception)
				{
					_logger.Error("Reading current value of '" + id + "' failed", exception);
					result[id] = null;
				}
			}
		}
		return result;
	}

	public async Task<IReadOnlyDictionary<string, TweakApplicability>> GetApplicabilityAsync(IEnumerable<string> tweakIds, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<string, TweakApplicability> result = new Dictionary<string, TweakApplicability>(StringComparer.Ordinal);
		foreach (string id in tweakIds)
		{
			ct.ThrowIfCancellationRequested();
			if (!_actions.TryGetValue(id, out ITweakAction value))
			{
				result[id] = TweakApplicability.No(_loc["applicability.notInBuild"]);
				continue;
			}
			try
			{
				Dictionary<string, TweakApplicability> dictionary = result;
				string key = id;
				dictionary[key] = await value.GetApplicabilityAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception exception)
			{
				_logger.Error("Applicability check for '" + id + "' failed", exception);
				result[id] = TweakApplicability.No(_loc["applicability.checkFailed"]);
			}
		}
		return result;
	}

	public async Task<IReadOnlyDictionary<string, bool>> GetAppliedStatesAsync(IEnumerable<string> tweakIds, CancellationToken ct = default(CancellationToken))
	{
		Dictionary<string, bool> result = new Dictionary<string, bool>(StringComparer.Ordinal);
		foreach (string id in tweakIds)
		{
			ct.ThrowIfCancellationRequested();
			if (_actions.TryGetValue(id, out ITweakAction value))
			{
				try
				{
					Dictionary<string, bool> dictionary = result;
					string key = id;
					dictionary[key] = await value.IsAppliedAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception exception)
				{
					_logger.Error("Applied-state check for '" + id + "' failed", exception);
					result[id] = false;
				}
			}
		}
		return result;
	}

	public async Task<ApplyOutcome> ApplyAsync(ApplyRequest request, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		ArgumentNullException.ThrowIfNull(request, "request");
		List<(string Id, ITweakAction Action, TweakDefinition Definition)> selected = request.TweakIds.Select((string text) => (Id: text, Action: _actions.GetValueOrDefault(text), Definition: TweakCatalog.Find(text))).ToList();
		List<TweakResult> results = new List<TweakResult>(selected.Count);
		List<TweakChangeRecord> records = new List<TweakChangeRecord>(selected.Count);
		long? restoreSequence = null;
		string restoreWarning = null;
		if (request.CreateRestorePoint && selected.Any(((string Id, ITweakAction Action, TweakDefinition Definition) s) => s.Action != null))
		{
			progress?.Report(_loc["engine.creatingRestorePoint"]);
			try
			{
				restoreSequence = await _restorePoints.CreateAsync(request.Title, ct).ConfigureAwait(continueOnCapturedContext: false);
				if (!restoreSequence.HasValue)
				{
					restoreWarning = _loc["engine.restorePointSkipped"];
				}
			}
			catch (Exception exception)
			{
				_logger.Error("Creating the restore point failed", exception);
				restoreWarning = _loc["engine.restorePointFailed"];
			}
		}
		List<string> backupFiles = new List<string>();
		if (request.BackupRegistry)
		{
			List<string> list = selected.Where(((string Id, ITweakAction Action, TweakDefinition Definition) s) => (object)s.Definition != null).SelectMany(((string Id, ITweakAction Action, TweakDefinition Definition) s) => s.Definition.BackupKeys).Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
			if (list.Count > 0)
			{
				progress?.Report(_loc["engine.backingUpRegistry"]);
				try
				{
					List<string> list2 = backupFiles;
					list2.AddRange(await _registryBackup.ExportAsync(list, request.Title, ct).ConfigureAwait(continueOnCapturedContext: false));
				}
				catch (Exception exception2)
				{
					_logger.Error("Registry backup failed", exception2);
				}
			}
		}
		foreach (var (id, action, definition) in selected)
		{
			ct.ThrowIfCancellationRequested();
			if (action == null)
			{
				results.Add(TweakResult.Fail(id, _loc["applicability.notInBuild"]));
				continue;
			}
			progress?.Report(((object)definition == null) ? id : _loc[definition.NameKey]);
			string before = await TryReadAsync(action, ct).ConfigureAwait(continueOnCapturedContext: false);
			TweakResult tweakResult;
			try
			{
				tweakResult = await action.ApplyAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				_logger.Error("Applying '" + id + "' failed", ex2);
				tweakResult = TweakResult.Fail(id, _loc.Format("result.unexpected", ex2.Message));
			}
			results.Add(tweakResult);
			_logger.TweakApplied(id, tweakResult.Success, tweakResult.PreviousValue, tweakResult.Message);
			if (tweakResult.Success)
			{
				List<TweakChangeRecord> list3 = records;
				TweakChangeRecord tweakChangeRecord = new TweakChangeRecord
				{
					TweakId = id,
					TweakName = (((object)definition == null) ? id : _loc[definition.NameKey]),
					PreviousValue = tweakResult.PreviousValue,
					Before = before,
					After = await TryReadAsync(action, ct).ConfigureAwait(continueOnCapturedContext: false)
				};
				list3.Add(tweakChangeRecord);
			}
		}
		ChangeSet changeSet = null;
		if (records.Count > 0)
		{
			changeSet = new ChangeSet
			{
				Id = Guid.NewGuid().ToString("N"),
				Title = request.Title,
				Category = request.Category,
				AppliedAt = DateTimeOffset.Now,
				Changes = records,
				RestorePointSequence = restoreSequence,
				RegistryBackupFiles = backupFiles
			};
			await _history.AppendAsync(changeSet, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		return new ApplyOutcome
		{
			Results = results,
			ChangeSet = changeSet,
			RestorePointWarning = restoreWarning
		};
	}

	public async Task<ApplyOutcome> RevertChangeSetAsync(ChangeSet changeSet, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		ArgumentNullException.ThrowIfNull(changeSet, "changeSet");
		List<TweakResult> results = new List<TweakResult>(changeSet.Changes.Count);
		List<TweakChangeRecord> updated = new List<TweakChangeRecord>(changeSet.Changes.Count);
		foreach (TweakChangeRecord record in changeSet.Changes.Reverse())
		{
			ct.ThrowIfCancellationRequested();
			if (record.Reverted)
			{
				updated.Add(record);
				continue;
			}
			if (!_actions.TryGetValue(record.TweakId, out ITweakAction value))
			{
				results.Add(TweakResult.Fail(record.TweakId, _loc["applicability.notInBuild"]));
				updated.Add(record);
				continue;
			}
			progress?.Report(record.TweakName);
			TweakResult tweakResult;
			try
			{
				tweakResult = await value.RevertAsync(record.PreviousValue, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				_logger.Error("Reverting '" + record.TweakId + "' failed", ex2);
				tweakResult = TweakResult.Fail(record.TweakId, _loc.Format("result.unexpected", ex2.Message));
			}
			results.Add(tweakResult);
			_logger.Info($"Revert '{record.TweakId}': {(tweakResult.Success ? "ok" : "failed")} — {tweakResult.Message}");
			updated.Add(tweakResult.Success ? record with
			{
				Reverted = true,
				RevertedAt = DateTimeOffset.Now
			} : record);
		}
		updated.Reverse();
		ChangeSet persisted = changeSet with
		{
			Changes = updated
		};
		await _history.UpdateAsync(persisted, ct).ConfigureAwait(continueOnCapturedContext: false);
		return new ApplyOutcome
		{
			Results = results,
			ChangeSet = persisted
		};
	}

	private async Task<string?> TryReadAsync(ITweakAction action, CancellationToken ct)
	{
		try
		{
			return await action.ReadCurrentValueAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception exception)
		{
			_logger.Error("Reading the state of '" + action.Id + "' for the history failed", exception);
			return null;
		}
	}

	public async Task<ApplyOutcome> ReapplyTweaksAsync(IReadOnlyList<string> tweakIds, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		ArgumentNullException.ThrowIfNull(tweakIds, "tweakIds");
		List<TweakResult> results = new List<TweakResult>(tweakIds.Count);
		foreach (string id in tweakIds)
		{
			ct.ThrowIfCancellationRequested();
			if (!_actions.TryGetValue(id, out ITweakAction value))
			{
				results.Add(TweakResult.Fail(id, _loc["applicability.notInBuild"]));
				continue;
			}
			TweakDefinition tweakDefinition = TweakCatalog.Find(id);
			progress?.Report(((object)tweakDefinition == null) ? id : _loc[tweakDefinition.NameKey]);
			TweakResult tweakResult;
			try
			{
				tweakResult = await value.ApplyAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				_logger.Error("Re-applying '" + id + "' failed", ex2);
				tweakResult = TweakResult.Fail(id, _loc.Format("result.unexpected", ex2.Message));
			}
			results.Add(tweakResult);
			_logger.Info($"Re-apply '{id}': {(tweakResult.Success ? "ok" : "failed")} — {tweakResult.Message}");
		}
		return new ApplyOutcome
		{
			Results = results
		};
	}

	public async Task<ApplyOutcome> RevertTweaksAsync(IReadOnlyList<string> tweakIds, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		ArgumentNullException.ThrowIfNull(tweakIds, "tweakIds");
		List<TweakResult> results = new List<TweakResult>();
		if (tweakIds.Count == 0)
		{
			return new ApplyOutcome
			{
				Results = results
			};
		}
		List<ChangeSet> history = (await _history.GetAllAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).OrderByDescending((ChangeSet c) => c.AppliedAt).ToList();
		foreach (string id in tweakIds)
		{
			ct.ThrowIfCancellationRequested();
			ChangeSet changeSet = history.FirstOrDefault((ChangeSet c) => c.Changes.Any((TweakChangeRecord r) => r.TweakId == id && !r.Reverted));
			if ((object)changeSet == null || !_actions.TryGetValue(id, out ITweakAction value))
			{
				results.Add(TweakResult.Fail(id, _loc["result.revert.noHistory"]));
				continue;
			}
			TweakChangeRecord record = changeSet.Changes.First((TweakChangeRecord r) => r.TweakId == id && !r.Reverted);
			progress?.Report(record.TweakName);
			TweakResult tweakResult;
			try
			{
				tweakResult = await value.RevertAsync(record.PreviousValue, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex2)
			{
				_logger.Error("Reverting '" + id + "' failed", ex2);
				tweakResult = TweakResult.Fail(id, _loc.Format("result.unexpected", ex2.Message));
			}
			results.Add(tweakResult);
			_logger.Info($"Revert '{id}': {(tweakResult.Success ? "ok" : "failed")} — {tweakResult.Message}");
			if (tweakResult.Success)
			{
				ChangeSet updated = changeSet with
				{
					Changes = changeSet.Changes.Select((TweakChangeRecord r) => ((object)r != record) ? r : r with
					{
						Reverted = true,
						RevertedAt = DateTimeOffset.Now
					}).ToList()
				};
				await _history.UpdateAsync(updated, ct).ConfigureAwait(continueOnCapturedContext: false);
				history[history.IndexOf(changeSet)] = updated;
			}
		}
		return new ApplyOutcome
		{
			Results = results
		};
	}
}
