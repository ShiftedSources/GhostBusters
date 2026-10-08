using System.IO;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class LargeFileViewModel(LargeFile file)
{
	public LargeFile File { get; } = file;

	public string Name => Path.GetFileName(File.Path);

	public string Folder => Path.GetDirectoryName(File.Path) ?? string.Empty;

	public string SizeLabel => GhostFormat.Bytes(File.Bytes);

	public string DateLabel => File.LastWrite.ToString("d", AppServices.Localizer.Culture);
}
