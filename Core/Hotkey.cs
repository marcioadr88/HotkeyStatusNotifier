using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace HotkeyStatusNotifier.Core;

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Win = 8
}

public sealed class Hotkey
{
    public HotkeyModifiers Modifiers { get; set; } = HotkeyModifiers.Control | HotkeyModifiers.Alt;
    public int VirtualKey { get; set; } = 0x4D;
    public ushort ScanCode { get; set; } = 0x32;
    public uint ExtendedFlags { get; set; }

    public static Hotkey Default => new();

    public bool HasModifiers => Modifiers != HotkeyModifiers.None;

    public static Hotkey FromVirtualKey(HotkeyModifiers modifiers, int vk)
    {
        var vsc = MapVirtualKey((uint)vk, 0);
        var ext = (vsc & 0xFF000000) == 0x01000000 ? 2u : 0u;
        return new Hotkey
        {
            Modifiers = modifiers,
            VirtualKey = vk,
            ScanCode = (ushort)(vsc & 0xFF),
            ExtendedFlags = ext
        };
    }

    public override string ToString()
    {
        var parts = new List<string>();
        if ((Modifiers & HotkeyModifiers.Control) != 0) parts.Add("Ctrl");
        if ((Modifiers & HotkeyModifiers.Alt) != 0) parts.Add("Alt");
        if ((Modifiers & HotkeyModifiers.Shift) != 0) parts.Add("Shift");
        if ((Modifiers & HotkeyModifiers.Win) != 0) parts.Add("Win");
        parts.Add(KeyName(VirtualKey));
        return string.Join("+", parts);
    }

    public static Hotkey Parse(string text)
    {
        var tokens = text.Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var mods = HotkeyModifiers.None;
        int? vk = null;
        foreach (var t in tokens)
        {
            var lower = t.ToLowerInvariant();
            switch (lower)
            {
                case "ctrl":
                case "control":
                    mods |= HotkeyModifiers.Control;
                    break;
                case "alt":
                    mods |= HotkeyModifiers.Alt;
                    break;
                case "shift":
                    mods |= HotkeyModifiers.Shift;
                    break;
                case "win":
                case "windows":
                    mods |= HotkeyModifiers.Win;
                    break;
                default:
                    if (vk == null) vk = KeyFromName(t);
                    break;
            }
        }
        if (vk == null) throw new FormatException("No key specified.");
        if (mods == HotkeyModifiers.None) throw new FormatException("At least one modifier is required.");
        return FromVirtualKey(mods, vk.Value);
    }

    private static int KeyFromName(string name)
    {
        if (name.Length == 1)
            return VkKeyScanW(name[0]) & 0xFF;

        var upper = name.ToUpperInvariant();
        var known = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["F1"] = 0x70, ["F2"] = 0x71, ["F3"] = 0x72, ["F4"] = 0x73,
            ["F5"] = 0x74, ["F6"] = 0x75, ["F7"] = 0x76, ["F8"] = 0x77,
            ["F9"] = 0x78, ["F10"] = 0x79, ["F11"] = 0x7A, ["F12"] = 0x7B,
            ["ENTER"] = 0x0D, ["RETURN"] = 0x0D, ["SPACE"] = 0x20, ["TAB"] = 0x09,
            ["ESC"] = 0x1B, ["ESCAPE"] = 0x1B, ["BACK"] = 0x08, ["BACKSPACE"] = 0x08,
            ["UP"] = 0x26, ["DOWN"] = 0x28, ["LEFT"] = 0x25, ["RIGHT"] = 0x27,
            ["HOME"] = 0x24, ["END"] = 0x23, ["PGUP"] = 0x21, ["PAGEDOWN"] = 0x22,
            ["DELETE"] = 0x2E, ["DEL"] = 0x2E, ["INSERT"] = 0x2D, ["INS"] = 0x2D
        };
        if (known.TryGetValue(upper, out var v)) return v;
        throw new FormatException($"Unknown key '{name}'.");
    }

    private static string KeyName(int vk)
    {
        if (vk >= 0x30 && vk <= 0x39) return ((char)vk).ToString();
        if (vk >= 0x41 && vk <= 0x5A) return ((char)vk).ToString();
        switch (vk)
        {
            case 0x0D: return "Enter";
            case 0x20: return "Space";
            case 0x09: return "Tab";
            case 0x1B: return "Esc";
            case 0x08: return "Backspace";
            case 0x26: return "Up";
            case 0x28: return "Down";
            case 0x25: return "Left";
            case 0x27: return "Right";
            case 0x24: return "Home";
            case 0x23: return "End";
            case 0x21: return "PgUp";
            case 0x22: return "PgDn";
            case 0x2E: return "Delete";
            case 0x2D: return "Insert";
        }
        if (vk >= 0x70 && vk <= 0x7B) return "F" + (vk - 0x70 + 1);
        return "Key " + vk;
    }

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern short VkKeyScanW(char ch);
}
