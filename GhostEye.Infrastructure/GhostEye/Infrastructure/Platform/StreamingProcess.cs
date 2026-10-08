using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GhostEye.Infrastructure.Platform;

public static class StreamingProcess
{
	public static async Task<ProcessResult> RunAsync(string fileName, string arguments, IProgress<string>? lines, Encoding? encoding = null, CancellationToken ct = default(CancellationToken))
	{
		ProcessStartInfo startInfo = new ProcessStartInfo
		{
			FileName = fileName,
			Arguments = arguments,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
			StandardOutputEncoding = (encoding ?? Encoding.UTF8),
			StandardErrorEncoding = (encoding ?? Encoding.UTF8)
		};
		using (Process process = new Process
		{
			StartInfo = startInfo
		})
		{
			StringBuilder output = new StringBuilder();
			StringBuilder errors = new StringBuilder();
			process.OutputDataReceived += (object _, DataReceivedEventArgs e) =>
			{
				OnLine(e.Data, output);
			};
			process.ErrorDataReceived += (object _, DataReceivedEventArgs e) =>
			{
				OnLine(e.Data, errors);
			};
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			try
			{
				await process.WaitForExitAsync(ct).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (OperationCanceledException)
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
				throw;
			}
			return new ProcessResult(process.ExitCode, output.ToString(), errors.ToString());
		}
		void OnLine(string? line, StringBuilder target)
		{
			if (line != null)
			{
				string[] array = line.Replace("\0", string.Empty).Split('\r', StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					string text = array[i].Trim();
					if (text.Length != 0)
					{
						target.AppendLine(text);
						lines?.Report(text);
					}
				}
			}
		}
	}
}
