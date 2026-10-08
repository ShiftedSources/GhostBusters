using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostEye.Core.Localization;

public static class Languages
{
	public const string English = "en";

	public const string Italian = "it";

	public const string German = "de";

	public const string French = "fr";

	public const string Spanish = "es";

	public const string Portuguese = "pt";

	public const string Polish = "pl";

	public static readonly IReadOnlyList<(string Code, string DisplayName)> All = new _003C_003Ez__ReadOnlyArray<(string, string)>(new (string, string)[7]
	{
		("en", "English"),
		("it", "Italiano"),
		("de", "Deutsch"),
		("fr", "Français"),
		("es", "Español"),
		("pt", "Português (Brasil)"),
		("pl", "Polski")
	});

	public static string CultureName(string code)
	{
		return code switch
		{
			"it" => "it-IT", 
			"de" => "de-DE", 
			"fr" => "fr-FR", 
			"es" => "es-ES", 
			"pt" => "pt-BR", 
			"pl" => "pl-PL", 
			_ => "en-US", 
		};
	}

	public static bool IsSupported(string? code)
	{
		if (code != null)
		{
			return All.Any(((string Code, string DisplayName) l) => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}
}
