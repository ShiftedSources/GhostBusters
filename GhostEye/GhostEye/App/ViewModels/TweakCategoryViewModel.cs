using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel.__Internals;
using GhostEye.App.Services;
using GhostEye.Core.Models;
using GhostEye.Core.Presets;

namespace GhostEye.App.ViewModels;

public sealed class TweakCategoryViewModel : ObservableObject, ILocalizedViewModel
{
	private readonly string _titleKey;

	public TweakCategory Category { get; }

	public string Title => AppServices.Localizer[_titleKey];

	public string IconKey { get; }

	public ObservableCollection<TweakItemViewModel> Items { get; } = new ObservableCollection<TweakItemViewModel>();

	public int SelectedCount => Items.Count((TweakItemViewModel i) => i.IsSelected);

	public string CountLabel => AppServices.Localizer.Format("optimize.count", SelectedCount, Items.Count);

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
				OnPropertyChanged(__KnownINotifyPropertyChangedArgs.IsExpanded);
			}
		}
	}

	public int PendingCount => IdsToApply.Count() + IdsToRevert.Count();

	public IEnumerable<string> SelectedIds => from i in Items
		where i.IsSelected
		select i.Id;

	public IEnumerable<string> IdsToApply => from i in Items
		where i.IsSelected && !i.IsAlreadyApplied
		select i.Id;

	public IEnumerable<string> IdsToRevert => from i in Items
		where !i.IsSelected && i.IsAlreadyApplied && i.IsApplicable
		select i.Id;

	public event EventHandler? SelectionChanged;

	public TweakCategoryViewModel(TweakCategory category, string titleKey, string iconKey, bool includeAdvanced = true)
	{
		TweakCategoryViewModel tweakCategoryViewModel = this;
		Category = category;
		_titleKey = titleKey;
		IconKey = iconKey;
		IsExpanded = true;
		foreach (TweakDefinition item in from d in TweakCatalog.ByCategory(category)
			where includeAdvanced || !d.AdvancedOnly
			select d)
		{
			TweakItemViewModel tweakItemViewModel = new TweakItemViewModel(item);
			tweakItemViewModel.PropertyChanged += (object? _, PropertyChangedEventArgs e) =>
			{
				string propertyName = e.PropertyName;
				if ((propertyName == "IsSelected" || propertyName == "IsAlreadyApplied") ? true : false)
				{
					tweakCategoryViewModel.OnPropertyChanged("SelectedCount");
					tweakCategoryViewModel.OnPropertyChanged("CountLabel");
					tweakCategoryViewModel.SelectionChanged?.Invoke(tweakCategoryViewModel, EventArgs.Empty);
				}
			};
			Items.Add(tweakItemViewModel);
		}
	}

	public void ApplySelection(IEnumerable<string> ids)
	{
		HashSet<string> hashSet = ids.ToHashSet(StringComparer.Ordinal);
		foreach (TweakItemViewModel item in Items)
		{
			item.IsSelected = item.IsApplicable && (hashSet.Contains(item.Id) || item.IsAlreadyApplied);
		}
	}

	public void ClearSelection()
	{
		foreach (TweakItemViewModel item in Items)
		{
			item.IsSelected = false;
		}
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Title");
		OnPropertyChanged("CountLabel");
		foreach (TweakItemViewModel item in Items)
		{
			item.OnLanguageChanged();
		}
	}
}
