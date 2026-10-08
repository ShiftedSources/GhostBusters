namespace GhostEye.Infrastructure.Platform;

public sealed record StartupEntry(string Name, string Command, StartupSource Source, bool IsEnabled, string Location, string? TaskPath = null)
{
	public bool IsDisabled => !IsEnabled;

	public string Id => $"{Source}|{TaskPath ?? Name}";

	public bool IsMachineWide
	{
		get
		{
			switch (Source)
			{
			case StartupSource.RunMachine:
			case StartupSource.RunMachine32:
			case StartupSource.FolderCommon:
			case StartupSource.ScheduledTask:
			case StartupSource.RemovedMachine:
			case StartupSource.RemovedMachine32:
			case StartupSource.RemovedFolderCommon:
				return true;
			default:
				return false;
			}
		}
	}

	public bool IsRemoved
	{
		get
		{
			StartupSource source = Source;
			if ((uint)(source - 6) <= 4u)
			{
				return true;
			}
			return false;
		}
	}

	public string? ExecutablePath => StartupCommand.ExtractExecutable(Command);
}
