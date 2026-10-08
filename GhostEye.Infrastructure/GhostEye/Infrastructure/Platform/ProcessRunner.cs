using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public sealed class ProcessRunner : IProcessRunner
{
	private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60.0);

	public async Task<ProcessResult> RunAsync(string fileName, string arguments, CancellationToken ct = default(CancellationToken))
	{
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding = Encoding.UTF8
		};
		using Process process = new Process
		{
			StartInfo = startInfo
		};
		StringBuilder stdout = new StringBuilder();
		StringBuilder stderr = new StringBuilder();
		process.OutputDataReceived += (object _, DataReceivedEventArgs e) =>
		{
			if (e.Data != null)
			{
				stdout.AppendLine(e.Data);
			}
		};
		process.ErrorDataReceived += (object _, DataReceivedEventArgs e) =>
		{
			if (e.Data != null)
			{
				stderr.AppendLine(e.Data);
			}
		};
		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();
		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeout.CancelAfter(DefaultTimeout);
		try
		{
			await process.WaitForExitAsync(timeout.Token).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException) when (!ct.IsCancellationRequested)
		{
			TryKill(process);
			return new ProcessResult(-1, stdout.ToString(), "Timeout: the command did not respond within 60 seconds.");
		}
		catch (OperationCanceledException)
		{
			TryKill(process);
			throw;
		}
		return new ProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString());
	}

	public Task<ProcessResult> RunPowerShellAsync(string command, CancellationToken ct = default(CancellationToken))
	{
		return RunAsync("powershell.exe", "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"" + command.Replace("\"", "\\\"") + "\"", ct);
	}

	private static void TryKill(Process process)
	{
		try
		{
			if (!process.HasExited)
			{
				process.Kill(entireProcessTree: true);
			}
		}
		catch (InvalidOperationException)
		{
		}
	}
}
