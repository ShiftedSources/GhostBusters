using System;
using System.IO;
using System.Text;
using GhostEye.Core.Abstractions;

namespace GhostEye.Backup;

public sealed class FileLogger : IAppLogger
{
	private readonly object _gate = new object();

	private readonly string _directory;

	private string CurrentFile => Path.Combine(_directory, $"ghosteye-{DateTime.Now:yyyyMMdd}.log");

	public FileLogger(string? directory = null)
	{
		_directory = directory ?? AppPaths.Logs;
	}

	public void Info(string message)
	{
		Write("INFO ", message);
	}

	public void Warn(string message)
	{
		Write("WARN ", message);
	}

	public void Error(string message, Exception? exception = null)
	{
		Write("ERROR", (exception == null) ? message : $"{message} :: {exception}");
	}

	public void TweakApplied(string tweakId, bool success, string? previousValue, string message)
	{
		string value = (success ? "OK" : "FAIL");
		string value2 = previousValue ?? "<assente>";
		Write("TWEAK", $"{tweakId} {value} previous={value2} {message}".TrimEnd());
	}

	private void Write(string level, string message)
	{
		string contents = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
		try
		{
			lock (_gate)
			{
				Directory.CreateDirectory(_directory);
				File.AppendAllText(CurrentFile, contents, Encoding.UTF8);
			}
		}
		catch (IOException)
		{
		}
		catch (UnauthorizedAccessException)
		{
		}
	}
}
