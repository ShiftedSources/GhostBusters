using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Disk;

namespace GhostEye.App.ViewModels;

public sealed class DuplicateGroupViewModel
{
	public DuplicateGroup Group { get; }

	public ObservableCollection<DuplicateFileViewModel> Files { get; }

	public string Title => AppServices.Localizer.Format("disk.dup.group", Path.GetFileName(Group.Files[0].Path), Group.Files.Count, GhostFormat.Bytes(Group.SizeBytes));

	public DuplicateGroupViewModel(DuplicateGroup group)
	{
		Group = group;
		Files = new ObservableCollection<DuplicateFileViewModel>(group.Files.Select((DuplicateFile f, int i) => new DuplicateFileViewModel(f, i == 0)));
	}
}
