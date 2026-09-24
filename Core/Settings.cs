namespace HotkeyStatusNotifier.Core;

public sealed class Settings
{
    public Hotkey Hotkey { get; set; } = Hotkey.Default;
    public int NotificationDurationMs { get; set; } = 1000;
    public int Volume { get; set; } = 70;
    public string Position { get; set; } = "BottomRight";
    public bool SoundEnabled { get; set; } = true;
    public bool VisualEnabled { get; set; } = true;
    public bool PersistentIndicator { get; set; }
    public string IndicatorSize { get; set; } = "Large";
    public int IndicatorMargin { get; set; } = 8;
    public bool ControlMicrophone { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public string InitialState { get; set; } = "Unmuted";
}
