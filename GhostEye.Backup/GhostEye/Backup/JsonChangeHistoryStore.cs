using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Models;

namespace GhostEye.Backup;

public sealed class JsonChangeHistoryStore : IChangeHistoryStore
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		Converters = { (JsonConverter)new JsonStringEnumConverter() }
	};

	private readonly SemaphoreSlim _gate = new SemaphoreSlim(1, 1);

	private readonly string _filePath;

	private readonly IAppLogger _logger;

	public JsonChangeHistoryStore(IAppLogger logger, string? filePath = null)
	{
		_logger = logger;
		_filePath = filePath ?? AppPaths.HistoryFile;
	}

	public async Task<IReadOnlyList<ChangeSet>> GetAllAsync(CancellationToken ct = default(CancellationToken))
	{
		await _gate.WaitAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			return await ReadUnsafeAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_gate.Release();
		}
	}

	public async Task AppendAsync(ChangeSet changeSet, CancellationToken ct = default(CancellationToken))
	{
		ArgumentNullException.ThrowIfNull(changeSet, "changeSet");
		await _gate.WaitAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			List<ChangeSet> list = (await ReadUnsafeAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).ToList();
			list.Insert(0, changeSet);
			await WriteUnsafeAsync(list, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_gate.Release();
		}
	}

	public async Task UpdateAsync(ChangeSet changeSet, CancellationToken ct = default(CancellationToken))
	{
		ArgumentNullException.ThrowIfNull(changeSet, "changeSet");
		await _gate.WaitAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			List<ChangeSet> list = (await ReadUnsafeAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).ToList();
			int num = list.FindIndex((ChangeSet c) => c.Id == changeSet.Id);
			if (num >= 0)
			{
				list[num] = changeSet;
			}
			else
			{
				list.Insert(0, changeSet);
			}
			await WriteUnsafeAsync(list, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_gate.Release();
		}
	}

	public async Task<bool> DeleteAsync(string changeSetId, CancellationToken ct = default(CancellationToken))
	{
		await _gate.WaitAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			List<ChangeSet> list = (await ReadUnsafeAsync(ct).ConfigureAwait(continueOnCapturedContext: false)).ToList();
			bool removed = list.RemoveAll((ChangeSet c) => c.Id == changeSetId) > 0;
			if (removed)
			{
				await WriteUnsafeAsync(list, ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			return removed;
		}
		finally
		{
			_gate.Release();
		}
	}

	public async Task ClearAsync(CancellationToken ct = default(CancellationToken))
	{
		await _gate.WaitAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			await WriteUnsafeAsync(Array.Empty<ChangeSet>(), ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		finally
		{
			_gate.Release();
		}
	}

	private async Task<IReadOnlyList<ChangeSet>> ReadUnsafeAsync(CancellationToken ct)
	{
		if (!File.Exists(_filePath))
		{
			return Array.Empty<ChangeSet>();
		}
		try
		{
			IReadOnlyList<ChangeSet> result;
			await using (FileStream stream = File.OpenRead(_filePath))
			{
				result = (await JsonSerializer.DeserializeAsync<List<ChangeSet>>(stream, SerializerOptions, ct).ConfigureAwait(continueOnCapturedContext: false)) ?? new List<ChangeSet>();
			}
			return result;
		}
		catch (JsonException exception)
		{
			string text = Path.Combine(Path.GetDirectoryName(_filePath), $"{Path.GetFileNameWithoutExtension(_filePath)}.corrupt-{DateTime.Now:yyyyMMdd-HHmmss}.json");
			try
			{
				File.Move(_filePath, text);
				_logger.Error("Change history is unreadable; it was moved to " + text + " and a new one will be started.", exception);
			}
			catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
			{
				_logger.Error("Change history is unreadable and could not be moved aside.", ex);
			}
			return Array.Empty<ChangeSet>();
		}
		catch (IOException exception2)
		{
			_logger.Error("Unable to read the change history.", exception2);
			return Array.Empty<ChangeSet>();
		}
	}

	private async Task WriteUnsafeAsync(IReadOnlyList<ChangeSet> all, CancellationToken ct)
	{
		string tempPath = _filePath + ".tmp";
		await using (FileStream stream = File.Create(tempPath))
		{
			await JsonSerializer.SerializeAsync(stream, all, SerializerOptions, ct).ConfigureAwait(continueOnCapturedContext: false);
		}
		File.Move(tempPath, _filePath, overwrite: true);
	}
}
