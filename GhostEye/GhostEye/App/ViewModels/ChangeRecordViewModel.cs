using System;
using System.Collections.Generic;
using GhostEye.App.Services;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;

namespace GhostEye.App.ViewModels;

public sealed class ChangeRecordViewModel(TweakChangeRecord record)
{
	public TweakChangeRecord Record { get; } = record;

	public string Name
	{
		get
		{
			TweakDefinition tweakDefinition = TweakCatalog.Find(Record.TweakId);
			if ((object)tweakDefinition == null)
			{
				return Record.TweakName;
			}
			return AppServices.Localizer[tweakDefinition.NameKey];
		}
	}

	public bool IsReverted => Record.Reverted;

	public string StatusLabel => AppServices.Localizer[Record.Reverted ? "history.change.reverted" : "history.change.active"];

	public string RevertedAtLabel
	{
		get
		{
			DateTimeOffset? revertedAt = Record.RevertedAt;
			if (revertedAt.HasValue)
			{
				DateTimeOffset valueOrDefault = revertedAt.GetValueOrDefault();
				return AppServices.Localizer.Format("history.change.revertedAt", GhostFormat.Date(valueOrDefault), GhostFormat.Time(valueOrDefault));
			}
			return string.Empty;
		}
	}

	public IReadOnlyList<ChangeDetailLine> Lines { get; } = ChangeDetails.Describe(record);

	public bool HasLines => Lines.Count > 0;
}
