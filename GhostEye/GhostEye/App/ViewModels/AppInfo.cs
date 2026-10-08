using System;

namespace GhostEye.App.ViewModels;

public static class AppInfo
{
	public const string Developer = "@SoloShown";

	public const string Website = "https://ghosteye.store";

	public const string WebsiteLabel = "ghosteye.store";

	public static string Version { get; }

	static AppInfo()
	{
		Version version = typeof(AppInfo).Assembly.GetName().Version;
		Version = (((object)version != null) ? $"{version.Major}.{version.Minor}.{version.Build}" : "0.0.0");
	}
}
