namespace GhostEye.Infrastructure.Platform;

public sealed record OperationResult(bool Success, string Error)
{
	public static OperationResult Ok { get; } = new OperationResult(Success: true, string.Empty);

	public static OperationResult Fail(string error)
	{
		return new OperationResult(Success: false, error);
	}
}
