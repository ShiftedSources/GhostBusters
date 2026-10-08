using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using GhostEye.App.Services;
using GhostEye.Core.Models;

namespace GhostEye.App.ViewModels;

public sealed class AnalysisGroupViewModel(AnalysisGroup group, string titleKey) : ObservableObject, ILocalizedViewModel
{
	public AnalysisGroup Group { get; } = group;

	public string Title => AppServices.Localizer[titleKey];

	public ObservableCollection<AnalysisItem> Items { get; } = new ObservableCollection<AnalysisItem>();

	public string CountLabel
	{
		get
		{
			if (Items.Count != 0)
			{
				return AppServices.Localizer.Format("analyzer.count", Items.Count((AnalysisItem i) => i.Status == AnalysisStatus.Ok), Items.Count);
			}
			return AppServices.Localizer["analyzer.noData"];
		}
	}

	public bool HasItems => Items.Count > 0;

	public void Replace(IEnumerable<AnalysisItem> items)
	{
		Items.Clear();
		foreach (AnalysisItem item in items)
		{
			Items.Add(item);
		}
		OnPropertyChanged("CountLabel");
		OnPropertyChanged("HasItems");
	}

	public void OnLanguageChanged()
	{
		OnPropertyChanged("Title");
		OnPropertyChanged("CountLabel");
	}
}
