namespace GhostEye.Infrastructure.Platform;

public enum StartupSource
{
	RunUser,
	RunMachine,
	RunMachine32,
	FolderUser,
	FolderCommon,
	ScheduledTask,
	RemovedUser,
	RemovedMachine,
	RemovedMachine32,
	RemovedFolderUser,
	RemovedFolderCommon
}
