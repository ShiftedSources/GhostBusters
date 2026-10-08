using System;
using System.Collections.Generic;
using System.Linq;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;
using GhostEye.Infrastructure.Tweaks;
using Microsoft.Win32;

namespace GhostEye.App.Services;

public static class ChangeDetails
{
	public static IReadOnlyList<ChangeDetailLine> Describe(TweakChangeRecord record)
	{
		return (from l in DescribeCore(record)
			select (!l.HasAfter && l.Before.Length != 0) ? l with
			{
				Before = AppServices.Localizer.Format("history.detail.wasOnly", l.Before)
			} : l).ToList();
	}

	private static IReadOnlyList<ChangeDetailLine> DescribeCore(TweakChangeRecord record)
	{
		string text = record.Before ?? record.PreviousValue;
		string after = record.After;
		IReadOnlyList<RegistryValueSnapshot> readOnlyList = RegistrySnapshotCodec.TryDecode(text);
		if (readOnlyList != null)
		{
			IReadOnlyList<RegistryValueSnapshot> afterSnapshot = RegistrySnapshotCodec.TryDecode(after) ?? Array.Empty<RegistryValueSnapshot>();
			return readOnlyList.Select((RegistryValueSnapshot b) =>
			{
				RegistryValueSnapshot registryValueSnapshot = afterSnapshot.FirstOrDefault((RegistryValueSnapshot x) => x.Key.Equals(b.Key, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(b.Name, StringComparison.OrdinalIgnoreCase));
				return new ChangeDetailLine(ShortenKey(b.Key) + " › " + ((b.Name.Length == 0) ? "(Default)" : b.Name), FormatValue(b), ((object)registryValueSnapshot == null) ? string.Empty : FormatValue(registryValueSnapshot));
			}).ToList();
		}
		if (text == null && after == null)
		{
			return Array.Empty<ChangeDetailLine>();
		}
		TweakDefinition tweakDefinition = TweakCatalog.Find(record.TweakId);
		return new _003C_003Ez__ReadOnlySingleElementList<ChangeDetailLine>(new ChangeDetailLine(((object)tweakDefinition != null) ? AppServices.Localizer[tweakDefinition.NameKey] : record.TweakName, FormatPlain(record.TweakId, text), FormatPlain(record.TweakId, after)));
	}

	private static string FormatValue(RegistryValueSnapshot snapshot)
	{
		if (!snapshot.Existed || snapshot.Value == null)
		{
			return AppServices.Localizer["history.detail.absent"];
		}
		switch (RegistrySnapshotCodec.ParseKind(snapshot.Kind))
		{
		case RegistryValueKind.Binary:
			return Hex(snapshot.Value);
		case RegistryValueKind.String:
		case RegistryValueKind.ExpandString:
			return "\"" + snapshot.Value + "\"";
		default:
			return snapshot.Value;
		}
	}

	private static string Hex(string base64)
	{
		try
		{
			byte[] array = Convert.FromBase64String(base64);
			string text = string.Join(' ', from b in array.Take(12)
				select b.ToString("X2"));
			return (array.Length > 12) ? (text + " …") : text;
		}
		catch (FormatException)
		{
			return base64;
		}
	}

	private static string FormatPlain(string tweakId, string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return string.Empty;
		}
		ILocalizer localizer = AppServices.Localizer;
		if (Guid.TryParse(value, out var result))
		{
			return AppServices.PowerPlans.GetSchemeName(result) ?? value;
		}
		switch (tweakId)
		{
		case "perf.memory-compression":
		{
			if (bool.TryParse(value, out var result3))
			{
				return localizer[result3 ? "history.value.compressionOn" : "history.value.compressionOff"];
			}
			break;
		}
		case "perf.storage-optimize":
		{
			if (bool.TryParse(value, out var result4))
			{
				return localizer[result4 ? "history.value.trimOff" : "history.value.trimOn"];
			}
			break;
		}
		case "perf.startup-cleanup":
		{
			if (int.TryParse(value, out var result2))
			{
				return localizer.Format("history.value.startupEntries", result2);
			}
			break;
		}
		}
		return (value.Length > 160) ? (value.Substring(0, 160) + " …") : value;
	}

	private static string ShortenKey(string key)
	{
		string[] array = key.Split('\\');
		if (array.Length > 4)
		{
			return $"{array[0]}\\…\\{array[^2]}\\{array[^1]}";
		}
		return key;
	}
}
