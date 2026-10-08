using System;

namespace GhostEye.Core.Abstractions;

public interface IAppLogger
{
	void Info(string message);

	void Warn(string message);

	void Error(string message, Exception? exception = null);

	void TweakApplied(string tweakId, bool success, string? previousValue, string message);
}
