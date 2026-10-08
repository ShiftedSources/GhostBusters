namespace GhostEye.Infrastructure.Platform;

public sealed record StartupOperationResult(bool Success, string Error)
{
	public static StartupOperationResult Ok { get; } = new StartupOperationResult(Success: true, string.Empty);

	public static StartupOperationResult Fail(string error)
	{
		return new StartupOperationResult(Success: false, error);
	}
}
