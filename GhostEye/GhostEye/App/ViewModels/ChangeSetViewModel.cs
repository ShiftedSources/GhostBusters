using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using CommunityToolkit.Mvvm.Input;
using GhostEye.App.Services;
using GhostEye.Core.Localization;
using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class ChangeSetViewModel : ObservableObject, ILocalizedViewModel
{
	private IReadOnlyList<ChangeRecordViewModel>? _records;

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	private RelayCommand? toggleDetailsCommand;

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public ChangeSet ChangeSet
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<GhostEye.Core.Models.ChangeSet>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.ChangeSet);
				field = value;
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.ChangeSet);
			}
		}
	}

	[ObservableProperty]
	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public bool IsExpanded
	{
		get
		{
			return field;
		}
		set
		{
			if (!EqualityComparer<bool>.Default.Equals(field, value))
			{
				OnPropertyChanging(__KnownINotifyPropertyChangingArgs.IsExpanded);
				field = value;
				OnIsExpandedChanged(value);
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsExpanded);
			}
		}
	}

	public string Title => ChangeSet.Title;

	public string Meta
	{
		get
		{
			if (ChangeSet.Changes.Count != 1)
			{
				return AppServices.Localizer.Format("history.meta", GhostFormat.Date(ChangeSet.AppliedAt), GhostFormat.Time(ChangeSet.AppliedAt), ChangeSet.Changes.Count);
			}
			return AppServices.Localizer.Format("history.metaOne", GhostFormat.Date(ChangeSet.AppliedAt), GhostFormat.Time(ChangeSet.AppliedAt));
		}
	}

	public TweakCategory Category => ChangeSet.Category;

	public int ActiveCount => ChangeSet.Changes.Count((TweakChangeRecord c) => !c.Reverted);

	public bool CanRestore => ActiveCount > 0;

	public bool IsPartiallyReverted
	{
		get
		{
			if (ActiveCount > 0)
			{
				return ActiveCount < ChangeSet.Changes.Count;
			}
			return false;
		}
	}

	public string PartialLabel => AppServices.Localizer.Format("history.status.partial", ChangeSet.Changes.Count - ActiveCount, ChangeSet.Changes.Count);

	public string RestoreLabel => AppServices.Localizer[ChangeSet.FullyReverted ? "history.restored" : "history.restore"];

	public string DetailsLabel => AppServices.Localizer[IsExpanded ? "history.details.hide" : "history.details.show"];

	public IReadOnlyList<ChangeRecordViewModel> Records => _records ?? (_records = ChangeSet.Changes.Select((TweakChangeRecord c) => new ChangeRecordViewModel(c)).ToList());

	public string SafetyLabel
	{
		get
		{
			ILocalizer localizer = AppServices.Localizer;
			List<string> list = new List<string>();
			long? restorePointSequence = ChangeSet.RestorePointSequence;
			string item;
			if (restorePointSequence.HasValue)
			{
				long valueOrDefault = restorePointSequence.GetValueOrDefault();
				item = localizer.Format("history.detail.restorePoint", valueOrDefault);
			}
			else
			{
				item = localizer["history.detail.noRestorePoint"];
			}
			list.Add(item);
			List<string> list2 = list;
			if (ChangeSet.RegistryBackupFiles.Count > 0)
			{
				list2.Add(localizer.Format("history.detail.registryBackup", ChangeSet.RegistryBackupFiles.Count));
			}
			return string.Join(" · ", list2);
		}
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.RelayCommandGenerator", "8.4.0.0")]
	[ExcludeFromCodeCoverage]
	public IRelayCommand ToggleDetailsCommand => toggleDetailsCommand ?? (toggleDetailsCommand = new RelayCommand(ToggleDetails));

	public ChangeSetViewModel(ChangeSet changeSet)
	{
		ChangeSet = changeSet;
	}

	[RelayCommand]
	private void ToggleDetails()
	{
		IsExpanded = !IsExpanded;
	}

	public void Refresh(ChangeSet updated)
	{
		ChangeSet = updated;
		_records = null;
		OnPropertyChanged("Records");
		OnLanguageChanged();
	}

	public void OnLanguageChanged()
	{
		_records = null;
		OnPropertyChanged("Records");
		OnPropertyChanged("Title");
		OnPropertyChanged("Meta");
		OnPropertyChanged("ActiveCount");
		OnPropertyChanged("CanRestore");
		OnPropertyChanged("IsPartiallyReverted");
		OnPropertyChanged("PartialLabel");
		OnPropertyChanged("RestoreLabel");
		OnPropertyChanged("DetailsLabel");
		OnPropertyChanged("SafetyLabel");
	}

	[GeneratedCode("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "8.4.0.0")]
	private void OnIsExpandedChanged(bool value)
	{
		OnPropertyChanged("DetailsLabel");
	}
}
