using System;
using System.Globalization;

namespace GhostEye.Core.Localization;

public interface ILocalizer
{
	string Language { get; }

	CultureInfo Culture { get; }

	string this[string key] { get; }

	event EventHandler? LanguageChanged;

	string Format(string key, params object?[] args);

	void SetLanguage(string languageCode);
}
