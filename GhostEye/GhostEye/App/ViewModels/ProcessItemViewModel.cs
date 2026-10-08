using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using GhostEye.App.Services;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.App.ViewModels;

public sealed class ProcessItemViewModel(ProcessUsage process) : ObservableObject
{
	public ProcessUsage Process { get; } = process;

	public string Name => Process.Name;

	public string MemoryLabel => GhostFormat.Bytes(Process.WorkingSetBytes);

	public string CpuLabel => $"{Process.CpuPercent:0.#}% CPU";

	public ImageSource? Icon { get; } = FileIcons.For(process.Path);

	public bool HasIcon => Icon != null;
}
