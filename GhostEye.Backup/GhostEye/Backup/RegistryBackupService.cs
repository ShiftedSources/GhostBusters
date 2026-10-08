using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Threading;
using System.Threading.Tasks;
using GhostEye.Core.Abstractions;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Backup;

public sealed class RegistryBackupService : IRegistryBackupService
{
	private readonly IProcessRunner _processRunner;

	private readonly IAppLogger _logger;

	public RegistryBackupService(IProcessRunner processRunner, IAppLogger logger)
	{
		_processRunner = processRunner;
		_logger = logger;
	}

	public async Task<IReadOnlyList<string>> ExportAsync(IEnumerable<string> registryKeys, string label, CancellationToken ct = default(CancellationToken))
	{
		string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
		string safeLabel = SanitizeFileName(label);
		List<string> written = new List<string>();
		foreach (string key in registryKeys.Distinct(StringComparer.OrdinalIgnoreCase))
		{
			ct.ThrowIfCancellationRequested();
			string path = $"{timestamp}_{safeLabel}_{SanitizeFileName(key)}.reg";
			string path2 = Path.Combine(AppPaths.Backups, path);
			ProcessResult processResult = await _processRunner.RunAsync("reg.exe", $"export \"{key}\" \"{path2}\" /y").ConfigureAwait(continueOnCapturedContext: false);
			if (processResult.Success && File.Exists(path2))
			{
				written.Add(path2);
			}
			else
			{
				_logger.Warn("Registry export failed for '" + key + "': " + processResult.Combined.Trim());
			}
		}
		return written;
	}

	public async Task<bool> ImportAsync(string regFilePath, CancellationToken ct = default(CancellationToken))
	{
		if (!File.Exists(regFilePath))
		{
			_logger.Warn("Backup file not found: " + regFilePath);
			return false;
		}
		ProcessResult processResult = await _processRunner.RunAsync("reg.exe", "import \"" + regFilePath + "\"", ct).ConfigureAwait(continueOnCapturedContext: false);
		if (!processResult.Success)
		{
			_logger.Error("Registry import failed from '" + regFilePath + "': " + processResult.Combined.Trim());
		}
		return processResult.Success;
	}

	private static string SanitizeFileName(string value)
	{
		string text = InvalidFileNameChars().Replace(value, "-").Trim('-');
		if (text.Length > 60)
		{
			return text.Substring(text.Length - 60);
		}
		return text;
	}

	[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.14.36720")]
	private static Regex InvalidFileNameChars()
	{
		return _003CRegexGenerator_g_003EF71E376FAD2752FBA54FD96890479470EE720A1C7871087FCE98491935D932B6C__InvalidFileNameChars_0.Instance;
	}
}
