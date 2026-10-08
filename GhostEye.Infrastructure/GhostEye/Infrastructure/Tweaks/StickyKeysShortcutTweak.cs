using System;
using System.Collections.Generic;
using System.Globalization;
using GhostEye.Core.Localization;
using GhostEye.Infrastructure.Platform;

namespace GhostEye.Infrastructure.Tweaks;

public sealed class StickyKeysShortcutTweak(IRegistryService registry, ILocalizer loc, Action<uint?, uint?, uint?>? applyToSession = null) : RegistryTweakBase("ui.sticky-keys-shortcut", registry, loc)
{
	private readonly Action<uint?, uint?, uint?> _applyToSession = applyToSession ?? new Action<uint?, uint?, uint?>(NativeUi.ApplyAccessibilityFlags);

	private const uint HotkeyActive = 4u;

	private const string StickyKeys = "HKCU\\Control Panel\\Accessibility\\StickyKeys";

	private const string FilterKeys = "HKCU\\Control Panel\\Accessibility\\Keyboard Response";

	private const string ToggleKeys = "HKCU\\Control Panel\\Accessibility\\ToggleKeys";

	private const uint StickyDefault = 510u;

	private const uint FilterDefault = 126u;

	private const uint ToggleDefault = 62u;

	protected override IReadOnlyList<RegistryWrite> Writes => new _003C_003Ez__ReadOnlyArray<RegistryWrite>(new RegistryWrite[3]
	{
		RegistryWrite.Text("HKCU\\Control Panel\\Accessibility\\StickyKeys", "Flags", WithoutHotkey("HKCU\\Control Panel\\Accessibility\\StickyKeys", 510u)),
		RegistryWrite.Text("HKCU\\Control Panel\\Accessibility\\Keyboard Response", "Flags", WithoutHotkey("HKCU\\Control Panel\\Accessibility\\Keyboard Response", 126u)),
		RegistryWrite.Text("HKCU\\Control Panel\\Accessibility\\ToggleKeys", "Flags", WithoutHotkey("HKCU\\Control Panel\\Accessibility\\ToggleKeys", 62u))
	});

	protected override void AfterApply()
	{
		_applyToSession(ReadFlags("HKCU\\Control Panel\\Accessibility\\StickyKeys"), ReadFlags("HKCU\\Control Panel\\Accessibility\\Keyboard Response"), ReadFlags("HKCU\\Control Panel\\Accessibility\\ToggleKeys"));
	}

	internal static uint ClearHotkey(uint flags)
	{
		return flags & 0xFFFFFFFBu;
	}

	private string WithoutHotkey(string key, uint fallback)
	{
		return ClearHotkey(ReadFlags(key) ?? fallback).ToString(CultureInfo.InvariantCulture);
	}

	private uint? ReadFlags(string key)
	{
		if (!uint.TryParse(Registry.ReadValue(key, "Flags")?.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return null;
		}
		return result;
	}
}
