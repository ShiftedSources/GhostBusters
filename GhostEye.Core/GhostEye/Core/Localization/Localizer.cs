using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace GhostEye.Core.Localization;

public sealed class Localizer : ILocalizer
{
	private readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> _tables;

	private readonly IReadOnlyDictionary<string, string> _english;

	private IReadOnlyDictionary<string, string> _active;

	public string Language { get; private set; }

	public CultureInfo Culture { get; private set; }

	public string this[string key]
	{
		get
		{
			if (_active.TryGetValue(key, out string value))
			{
				return value;
			}
			if (!_english.TryGetValue(key, out string value2))
			{
				return key;
			}
			return value2;
		}
	}

	public event EventHandler? LanguageChanged;

	public Localizer(string languageCode = "en")
	{
		_tables = Languages.All.ToDictionary(((string Code, string DisplayName) l) => l.Code, ((string Code, string DisplayName) l) => Load(l.Code), StringComparer.Ordinal);
		_english = _tables["en"];
		Language = Normalise(languageCode);
		_active = _tables[Language];
		Culture = CultureInfo.GetCultureInfo(Languages.CultureName(Language));
	}

	public string Format(string key, params object?[] args)
	{
		string text = this[key];
		try
		{
			return string.Format(Culture, text, args);
		}
		catch (FormatException)
		{
			return text;
		}
	}

	public void SetLanguage(string languageCode)
	{
		string text = Normalise(languageCode);
		if (!(text == Language))
		{
			Language = text;
			_active = _tables[text];
			Culture = CultureInfo.GetCultureInfo(Languages.CultureName(text));
			LanguageChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	private static string Normalise(string? code)
	{
		if (!Languages.IsSupported(code))
		{
			return "en";
		}
		return code.ToLowerInvariant();
	}

	private static IReadOnlyDictionary<string, string> Load(string languageCode)
	{
		Assembly assembly = typeof(Localizer).Assembly;
		string name = "GhostEye.Core.Localization.Strings." + languageCode + ".json";
		using Stream stream = assembly.GetManifestResourceStream(name);
		if (stream == null)
		{
			return new Dictionary<string, string>(StringComparer.Ordinal);
		}
		try
		{
			Dictionary<string, string> dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(stream);
			return (dictionary == null) ? new Dictionary<string, string>(StringComparer.Ordinal) : new Dictionary<string, string>(dictionary, StringComparer.Ordinal);
		}
		catch (JsonException)
		{
			return new Dictionary<string, string>(StringComparer.Ordinal);
		}
	}

	public IReadOnlyCollection<string> KeysFor(string languageCode)
	{
		if (!_tables.TryGetValue(languageCode, out IReadOnlyDictionary<string, string> value))
		{
			return new List<string>();
		}
		return value.Keys.ToList();
	}

	public string? RawLookup(string languageCode, string key)
	{
		if (!_tables.TryGetValue(languageCode, out IReadOnlyDictionary<string, string> value))
		{
			return null;
		}
		return value.GetValueOrDefault(key);
	}
}
