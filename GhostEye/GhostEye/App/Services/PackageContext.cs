using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GhostEye.App.Services;

public static class PackageContext
{
	private const int AppModelErrorNoPackage = 15700;

	private const int ErrorInsufficientBuffer = 122;

	public static bool IsPackaged { get; } = DetectPackage();

	public static string? FamilyName { get; } = ReadFamilyName();

	public static string? ActivationPath
	{
		get
		{
			if (FamilyName != null)
			{
				return "shell:AppsFolder\\" + FamilyName + "!GhostEye";
			}
			return null;
		}
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
	private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder? packageFullName);

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
	private static extern int GetCurrentPackageFamilyName(ref int packageFamilyNameLength, StringBuilder? packageFamilyName);

	private static string? ReadFamilyName()
	{
		if (!IsPackaged)
		{
			return null;
		}
		int packageFamilyNameLength = 0;
		if (GetCurrentPackageFamilyName(ref packageFamilyNameLength, null) != 122)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder(packageFamilyNameLength);
		if (GetCurrentPackageFamilyName(ref packageFamilyNameLength, stringBuilder) != 0)
		{
			return null;
		}
		return stringBuilder.ToString();
	}

	private static bool DetectPackage()
	{
		try
		{
			int packageFullNameLength = 0;
			return GetCurrentPackageFullName(ref packageFullNameLength, null) != 15700;
		}
		catch (EntryPointNotFoundException)
		{
			return false;
		}
	}
}
