using System;
using Microsoft.Win32;

namespace HotkeyStatusNotifier.Services;

public static class StartWithWindows
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "HotkeyStatusNotifier";

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
            return key?.GetValue(ValueName) is string;
        }
        catch
        {
            return false;
        }
    }

    public static void SetEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
            if (enable)
            {
                var exe = Environment.ProcessPath ?? string.Empty;
                key.SetValue(ValueName, "\"" + exe + "\"");
            }
            else
            {
                key.DeleteValue(ValueName, throwOnMissingValue: false);
            }
        }
        catch
        {
        }
    }
}
