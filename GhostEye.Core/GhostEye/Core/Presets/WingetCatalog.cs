using System;
using System.Collections.Generic;
using System.Linq;

namespace GhostEye.Core.Presets;

public static class WingetCatalog
{
	public static readonly IReadOnlyList<WingetPackage> Packages = new _003C_003Ez__ReadOnlyArray<WingetPackage>(new WingetPackage[22]
	{
		new WingetPackage("Google.Chrome", "Google Chrome", "browsers", "^Google Chrome$"),
		new WingetPackage("Mozilla.Firefox", "Mozilla Firefox", "browsers", "^Mozilla Firefox\\b"),
		new WingetPackage("Brave.Brave", "Brave", "browsers", "^Brave$"),
		new WingetPackage("Valve.Steam", "Steam", "gaming", "^Steam$"),
		new WingetPackage("EpicGames.EpicGamesLauncher", "Epic Games Launcher", "gaming", "^Epic Games Launcher$"),
		new WingetPackage("GOG.Galaxy", "GOG Galaxy", "gaming", "^GOG Galaxy\\b"),
		new WingetPackage("ElectronicArts.EADesktop", "EA app", "gaming", "^EA app$"),
		new WingetPackage("Ubisoft.Connect", "Ubisoft Connect", "gaming", "^Ubisoft Connect$"),
		new WingetPackage("Discord.Discord", "Discord", "gaming", "^Discord$"),
		new WingetPackage("Guru3D.Afterburner", "MSI Afterburner", "gaming", "^MSI Afterburner\\b"),
		new WingetPackage("VideoLAN.VLC", "VLC media player", "media", "^VLC media player$"),
		new WingetPackage("Spotify.Spotify", "Spotify", "media", "^Spotify$"),
		new WingetPackage("OBSProject.OBSStudio", "OBS Studio", "media", "^OBS Studio$"),
		new WingetPackage("7zip.7zip", "7-Zip", "utilities", "^7-Zip\\b"),
		new WingetPackage("Notepad++.Notepad++", "Notepad++", "utilities", "^Notepad\\+\\+"),
		new WingetPackage("Microsoft.PowerToys", "Microsoft PowerToys", "utilities", "^PowerToys\\b"),
		new WingetPackage("voidtools.Everything", "Everything", "utilities", "^Everything \\d"),
		new WingetPackage("Microsoft.VisualStudioCode", "Visual Studio Code", "developer", "^Microsoft Visual Studio Code\\b"),
		new WingetPackage("Git.Git", "Git", "developer", "^Git$|^Git version \\d"),
		new WingetPackage("Microsoft.VCRedist.2015+.x64", "Visual C++ Redistributable (x64)", "runtimes", "^Microsoft Visual C\\+\\+ 2015-20\\d\\d Redistributable \\(x64\\)"),
		new WingetPackage("Microsoft.DotNet.DesktopRuntime.8", ".NET Desktop Runtime 8", "runtimes", "^Microsoft Windows Desktop Runtime - 8\\.\\d+\\.\\d+ \\(x64\\)"),
		new WingetPackage("Microsoft.DirectX", "DirectX End-User Runtime", "runtimes")
	});

	public static IReadOnlyList<string> Groups { get; } = Packages.Select((WingetPackage p) => p.Group).Distinct(StringComparer.Ordinal).ToList();
}
