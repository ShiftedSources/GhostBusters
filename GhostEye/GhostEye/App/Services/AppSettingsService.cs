using System;
using System.IO;
using System.Text.Json;
using GhostEye.Backup;
using GhostEye.Core.Abstractions;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Tweaks;

namespace GhostEye.App.Services;

public sealed class AppSettingsService : IDnsPreferenceProvider
{
	private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	private readonly IAppLogger _logger;

	private readonly string _filePath;

	public AppSettings Current { get; private set; }

	public string PrimaryDns => Current.PrimaryDns;

	public string SecondaryDns => Current.SecondaryDns;

	public AppSettingsService(IAppLogger logger, string? filePath = null)
	{
		_logger = logger;
		_filePath = filePath ?? AppPaths.SettingsFile;
		Current = Load();
	}

	public static string PeekLanguage()
	{
		try
		{
			string settingsFile = AppPaths.SettingsFile;
			if (!File.Exists(settingsFile))
			{
				return "en";
			}
			AppSettings appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingsFile), SerializerOptions);
			return Languages.IsSupported(appSettings?.Language) ? appSettings.Language : "en";
		}
		catch (Exception ex) when ((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			return "en";
		}
	}

	public void Update(Func<AppSettings, AppSettings> change)
	{
		Current = change(Current);
		Save();
	}

	private AppSettings Load()
	{
		if (!File.Exists(_filePath))
		{
			return new AppSettings();
		}
		try
		{
			return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(_filePath), SerializerOptions) ?? new AppSettings();
		}
		catch (Exception ex) when ((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			_logger.Warn("Settings unreadable, defaults restored.");
			return new AppSettings();
		}
	}

	private void Save()
	{
		try
		{
			File.WriteAllText(_filePath, JsonSerializer.Serialize(Current, SerializerOptions));
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			_logger.Warn("Could not save settings.");
		}
	}
}
