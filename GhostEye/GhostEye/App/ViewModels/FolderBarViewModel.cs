using GhostEye.App.Services;
using GhostEye.Infrastructure.Disk;

namespace GhostEye.App.ViewModels;

public sealed class FolderBarViewModel(FolderUsage usage, long largest, long total)
{
	public FolderUsage Usage { get; } = usage;

	public string Name
	{
		get
		{
			if (!Usage.IsFilesEntry)
			{
				return Usage.Name;
			}
			return AppServices.Localizer["disk.map.looseFiles"];
		}
	}

	public string SizeLabel
	{
		get
		{
			if (!Usage.IsInaccessible)
			{
				return GhostFormat.Bytes(Usage.Bytes);
			}
			return AppServices.Localizer["disk.map.denied"];
		}
	}

	public string ShareLabel
	{
		get
		{
			if (total != 0L)
			{
				return $"{100.0 * (double)Usage.Bytes / (double)total:0.#}%";
			}
			return string.Empty;
		}
	}

	public string FilesLabel => AppServices.Localizer.Format("disk.map.files", Usage.Files.ToString("N0", AppServices.Localizer.Culture));

	public double Ratio
	{
		get
		{
			if (largest != 0L)
			{
				return (double)Usage.Bytes / (double)largest;
			}
			return 0.0;
		}
	}

	public bool CanOpen
	{
		get
		{
			if (!Usage.IsFilesEntry)
			{
				return !Usage.IsInaccessible;
			}
			return false;
		}
	}
}
