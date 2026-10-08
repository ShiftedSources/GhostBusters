using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Games;

public sealed class FrameCaptureService(string presentMonPath, ILocalizer loc)
{
	private const string SessionName = "GhostEyeCapture";

	public async Task<FrameCaptureResult> CaptureAsync(int processId, int seconds, CancellationToken ct = default(CancellationToken))
	{
		if (!File.Exists(presentMonPath))
		{
			return new FrameCaptureResult(Success: false, null, loc["fps.error.missingTool"]);
		}
		string csv = Path.Combine(Path.GetTempPath(), $"ghosteye-frames-{Guid.NewGuid():N}.csv");
		string arguments = string.Join(' ', "--process_id", processId.ToString(CultureInfo.InvariantCulture), "--output_file", "\"" + csv + "\"", "--timed", seconds.ToString(CultureInfo.InvariantCulture), "--terminate_after_timed", "--terminate_on_proc_exit", "--no_console_stats", "--v1_metrics", "--session_name", "GhostEyeCapture", "--stop_existing_session");
		try
		{
			ProcessResult processResult = await StreamingProcess.RunAsync(presentMonPath, arguments, null, null, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (!File.Exists(csv))
			{
				string combined = processResult.Combined;
				return new FrameCaptureResult(Success: false, null, (combined.Contains("access", StringComparison.OrdinalIgnoreCase) || combined.Contains("elevat", StringComparison.OrdinalIgnoreCase)) ? loc["fps.error.admin"] : loc["fps.error.noFrames"]);
			}
			FrameStats frameStats = Analyze(await File.ReadAllLinesAsync(csv, ct).ConfigureAwait(continueOnCapturedContext: false));
			return ((object)frameStats == null) ? new FrameCaptureResult(Success: false, null, loc["fps.error.noFrames"]) : new FrameCaptureResult(Success: true, frameStats, string.Empty);
		}
		catch (Win32Exception ex)
		{
			return new FrameCaptureResult(Success: false, null, ex.Message);
		}
		finally
		{
			try
			{
				File.Delete(csv);
			}
			catch (Exception ex2) when ((ex2 is IOException || ex2 is UnauthorizedAccessException) ? true : false)
			{
			}
		}
	}

	public static FrameStats? Analyze(IReadOnlyList<string> lines)
	{
		if (lines.Count < 2)
		{
			return null;
		}
		string[] header = lines[0].Split(',');
		int num = FindColumn(header, "msBetweenPresents", "MsBetweenPresents", "FrameTime");
		int num2 = FindColumn(header, "SwapChainAddress");
		if (num < 0)
		{
			return null;
		}
		Dictionary<string, List<double>> dictionary = new Dictionary<string, List<double>>(StringComparer.Ordinal);
		foreach (string item in lines.Skip(1))
		{
			string[] array = item.Split(',');
			if (array.Length > num && double.TryParse(array[num], NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && !(result <= 0.0) && !(result > 5000.0))
			{
				string key = ((num2 >= 0 && array.Length > num2) ? array[num2] : string.Empty);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = (dictionary[key] = new List<double>());
				}
				value.Add(result);
			}
		}
		List<double> list2 = dictionary.Values.OrderByDescending((List<double> l) => l.Count).FirstOrDefault();
		if (list2 == null || list2.Count < 10)
		{
			return null;
		}
		double num3 = list2.Sum();
		List<double> sorted = list2.OrderByDescending((double f) => f).ToList();
		return new FrameStats(list2.Count, num3 / 1000.0, (double)list2.Count / (num3 / 1000.0), LowFps(0.01), LowFps(0.001), num3 / (double)list2.Count);
		double LowFps(double fraction)
		{
			int count = Math.Max(1, (int)Math.Ceiling((double)sorted.Count * fraction));
			return 1000.0 / sorted.Take(count).Average();
		}
	}

	private static int FindColumn(string[] header, params string[] names)
	{
		foreach (string name in names)
		{
			int num = Array.FindIndex(header, (string h) => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
			if (num >= 0)
			{
				return num;
			}
		}
		return -1;
	}
}
