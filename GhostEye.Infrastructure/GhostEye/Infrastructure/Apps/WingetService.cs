using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Apps;

public sealed class WingetService
{
	private const string Agreements = "--accept-source-agreements --disable-interactivity";

	public async Task<bool> IsAvailableAsync(CancellationToken ct = default(CancellationToken))
	{
		try
		{
			return (await StreamingProcess.RunAsync("winget", "--version", null, null, ct).ConfigureAwait(continueOnCapturedContext: false)).Success;
		}
		catch (Win32Exception)
		{
			return false;
		}
	}

	public async Task<IReadOnlySet<string>> GetInstalledIdsAsync(CancellationToken ct = default(CancellationToken))
	{
		string file = Path.Combine(Path.GetTempPath(), $"ghosteye-winget-{Guid.NewGuid():N}.json");
		try
		{
			await StreamingProcess.RunAsync("winget", "export -o \"" + file + "\" --accept-source-agreements --disable-interactivity", null, null, ct).ConfigureAwait(continueOnCapturedContext: false);
			if (!File.Exists(file))
			{
				return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(await File.ReadAllTextAsync(file, ct).ConfigureAwait(continueOnCapturedContext: false));
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (jsonDocument.RootElement.TryGetProperty("Sources", out var value))
			{
				foreach (JsonElement item in value.EnumerateArray())
				{
					if (!item.TryGetProperty("Packages", out var value2))
					{
						continue;
					}
					foreach (JsonElement item2 in value2.EnumerateArray())
					{
						if (item2.TryGetProperty("PackageIdentifier", out var value3))
						{
							string text = value3.GetString();
							if (text != null)
							{
								hashSet.Add(text);
							}
						}
					}
				}
			}
			return hashSet;
		}
		catch (Exception ex) when ((ex is Win32Exception || ex is JsonException || ex is IOException) ? true : false)
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		finally
		{
			TryDelete(file);
		}
	}

	public async Task<IReadOnlyList<WingetUpgrade>> GetUpgradesAsync(CancellationToken ct = default(CancellationToken))
	{
		try
		{
			return ParseUpgradeTable((await StreamingProcess.RunAsync("winget", "upgrade --accept-source-agreements --disable-interactivity", null, null, ct).ConfigureAwait(continueOnCapturedContext: false)).StandardOutput);
		}
		catch (Win32Exception)
		{
			return Array.Empty<WingetUpgrade>();
		}
	}

	public Task<WingetResult> InstallAsync(string id, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		return RunAsync("install --id " + Quote(id) + " --exact --silent --accept-package-agreements --accept-source-agreements --disable-interactivity", progress, ct);
	}

	public Task<WingetResult> UpgradeAsync(string id, IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		return RunAsync("upgrade --id " + Quote(id) + " --exact --silent --accept-package-agreements --accept-source-agreements --disable-interactivity", progress, ct);
	}

	public Task<WingetResult> UpgradeAllAsync(IProgress<string>? progress = null, CancellationToken ct = default(CancellationToken))
	{
		return RunAsync("upgrade --all --silent --accept-package-agreements --accept-source-agreements --disable-interactivity", progress, ct);
	}

	private static async Task<WingetResult> RunAsync(string arguments, IProgress<string>? progress, CancellationToken ct)
	{
		try
		{
			string last = null;
			Progress<string> lines = new Progress<string>((string line) =>
			{
				if (line.Any(char.IsLetter))
				{
					last = line;
					progress?.Report(line);
				}
			});
			return new WingetResult((await StreamingProcess.RunAsync("winget", arguments, lines, null, ct).ConfigureAwait(continueOnCapturedContext: false)).Success, last ?? string.Empty);
		}
		catch (Win32Exception ex)
		{
			return new WingetResult(Success: false, ex.Message);
		}
	}

	public static IReadOnlyList<WingetUpgrade> ParseUpgradeTable(string output)
	{
		string[] array = output.Replace("\r\n", "\n").Split('\n');
		int num = Array.FindIndex(array, (string l) => l.Trim().Length >= 10 && l.Trim().All((char c) => c == '-'));
		if (num < 1)
		{
			return Array.Empty<WingetUpgrade>();
		}
		string text = array[num - 1];
		List<int> starts = new List<int>();
		for (int num2 = 0; num2 < text.Length; num2++)
		{
			if (text[num2] != ' ' && (num2 == 0 || text[num2 - 1] == ' '))
			{
				starts.Add(num2);
			}
		}
		if (starts.Count < 4)
		{
			return Array.Empty<WingetUpgrade>();
		}
		List<WingetUpgrade> list = new List<WingetUpgrade>();
		foreach (string item in array.Skip(num + 1))
		{
			if (string.IsNullOrWhiteSpace(item))
			{
				break;
			}
			string text2 = Cell(item, 1);
			if (text2.Length != 0 && !text2.Contains(' '))
			{
				list.Add(new WingetUpgrade(Cell(item, 0), text2, Cell(item, 2), Cell(item, 3)));
			}
		}
		return list;
		string Cell(string line, int column)
		{
			int num3 = starts[column];
			if (num3 >= line.Length)
			{
				return string.Empty;
			}
			int num4 = ((column + 1 < starts.Count) ? Math.Min(starts[column + 1], line.Length) : line.Length);
			int num5 = num3;
			return line.Substring(num5, num4 - num5).Trim();
		}
	}

	private static string Quote(string id)
	{
		return "\"" + id.Replace("\"", string.Empty) + "\"";
	}

	private static void TryDelete(string file)
	{
		try
		{
			File.Delete(file);
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
		}
	}
}
