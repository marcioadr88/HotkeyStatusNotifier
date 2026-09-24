using System;
using System.IO;
using System.Text.Json;

namespace HotkeyStatusNotifier.Core;

public sealed class SettingsService
{
    public const string AppName = "HotkeyStatusNotifier";

    public static string DataDir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);

    public static string SettingsPath => Path.Combine(DataDir, "settings.json");
    public static string ErrorLogPath => Path.Combine(DataDir, "error.log");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public Settings Current { get; private set; } = new Settings();

    public event EventHandler? SettingsChanged;

    public void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var loaded = JsonSerializer.Deserialize<Settings>(json, JsonOptions);
                if (loaded != null)
                {
                    Current = Sanitize(loaded);
                    return;
                }
            }
        }
        catch
        {
        }

        Current = Sanitize(new Settings());
    }

    private static Settings Sanitize(Settings s)
    {
        if (s.NotificationDurationMs is < 300 or > 10000) s.NotificationDurationMs = 1000;
        s.Volume = Math.Clamp(s.Volume, 0, 100);
        if (!IsValidPosition(s.Position)) s.Position = "BottomRight";
        if (s.IndicatorSize is not ("Large" or "Medium" or "Small")) s.IndicatorSize = "Large";
        s.IndicatorMargin = Math.Clamp(s.IndicatorMargin, -40, 40);
        if (s.InitialState is not ("Unmuted" or "Muted")) s.InitialState = "Unmuted";
        if (s.Hotkey is null || !s.Hotkey.HasModifiers || s.Hotkey.VirtualKey == 0 || s.Hotkey.ScanCode == 0)
            s.Hotkey = Hotkey.Default;
        return s;
    }

    private static bool IsValidPosition(string p) =>
        p is "BottomRight" or "BottomLeft" or "TopRight" or "TopLeft" or "CenterRight" or "CenterLeft";

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(DataDir);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(Current, JsonOptions));
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        catch
        {
        }
    }
}
