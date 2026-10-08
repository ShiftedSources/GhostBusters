using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace GhostEye.Infrastructure.Games;

public sealed class FrameCaptureLog(string path)
{
	private const int MaxEntries = 100;

	public IReadOnlyList<FrameCaptureEntry> GetAll()
	{
		try
		{
			return File.Exists(path) ? (JsonSerializer.Deserialize<List<FrameCaptureEntry>>(File.ReadAllText(path)) ?? new List<FrameCaptureEntry>()) : new List<FrameCaptureEntry>();
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException) ? true : false)
		{
			return Array.Empty<FrameCaptureEntry>();
		}
	}

	public void Add(FrameCaptureEntry entry)
	{
		Save(GetAll().Prepend(entry).Take(100).ToList());
	}

	public void Delete(string id)
	{
		Save((from e in GetAll()
			where e.Id != id
			select e).ToList());
	}

	public FrameCaptureEntry? PreviousOf(FrameCaptureEntry entry)
	{
		return (from e in GetAll()
			where e.Id != entry.Id && e.GameName == entry.GameName && e.CapturedAt < entry.CapturedAt
			orderby e.CapturedAt descending
			select e).FirstOrDefault();
	}

	private void Save(IReadOnlyList<FrameCaptureEntry> entries)
	{
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(path, JsonSerializer.Serialize(entries));
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
	}
}
