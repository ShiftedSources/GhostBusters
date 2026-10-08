using System;
using System.IO;
using System.Security.Cryptography;
using GhostEye.Backup;

namespace GhostEye.App.Services;

public static class FrameCaptureTool
{
	private const string ResourceName = "GhostEye.Tools.PresentMon.exe";

	private const string ExpectedHash = "B2A706BC6AD475749E3B7E3409263AA1E6906D45BDCF993F6DBC0F660188F1AF";

	public static string Path { get; } = System.IO.Path.Combine(AppPaths.Root, "tools", "PresentMon-2.6.0.exe");

	public static string? Ensure()
	{
		try
		{
			if (File.Exists(Path) && HashOf(Path) == "B2A706BC6AD475749E3B7E3409263AA1E6906D45BDCF993F6DBC0F660188F1AF")
			{
				return Path;
			}
			using Stream stream = typeof(FrameCaptureTool).Assembly.GetManifestResourceStream("GhostEye.Tools.PresentMon.exe");
			if (stream == null)
			{
				return null;
			}
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
			string text = Path + ".tmp";
			using (FileStream destination = File.Create(text))
			{
				stream.CopyTo(destination);
			}
			if (HashOf(text) != "B2A706BC6AD475749E3B7E3409263AA1E6906D45BDCF993F6DBC0F660188F1AF")
			{
				File.Delete(text);
				return null;
			}
			File.Move(text, Path, overwrite: true);
			return Path;
		}
		catch (Exception ex) when ((ex is IOException || ex is UnauthorizedAccessException) ? true : false)
		{
			AppServices.Logger.Error("Could not prepare PresentMon", ex);
			return null;
		}
	}

	private static string HashOf(string file)
	{
		using FileStream source = File.OpenRead(file);
		return Convert.ToHexString(SHA256.HashData(source));
	}
}
