using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using HotkeyStatusNotifier.Core;
using HotkeyStatusNotifier.Services;

namespace HotkeyStatusNotifier.UI;

public partial class SettingsWindow : Window
{
    private static readonly string[] Positions = { "BottomRight", "BottomLeft", "TopRight", "TopLeft", "CenterRight", "CenterLeft" };
    private static readonly string[] IndicatorSizes = { "Large", "Medium", "Small" };
    private static readonly string[] InitialStates = { "Unmuted", "Muted" };

    private readonly SettingsService _settings;
    private readonly HotkeyMonitor _monitor;
    private readonly SoundService _sound;
    private Hotkey? _capturedHotkey;

    public SettingsWindow(SettingsService settings, HotkeyMonitor monitor, VoiceState state, SoundService sound)
    {
        InitializeComponent();
        _settings = settings;
        _monitor = monitor;
        _sound = sound;

        var s = _settings.Current;
        HotkeyBox.Text = s.Hotkey.ToString();
        SoundEnabledBox.IsChecked = s.SoundEnabled;
        VolumeSlider.Value = s.Volume;
        VisualEnabledBox.IsChecked = s.VisualEnabled;
        PersistentIndicatorBox.IsChecked = s.PersistentIndicator;
        ControlMicBox.IsChecked = s.ControlMicrophone;
        DurationBox.Text = s.NotificationDurationMs.ToString();
        PositionCombo.SelectedIndex = Array.IndexOf(Positions, s.Position);
        if (PositionCombo.SelectedIndex < 0) PositionCombo.SelectedIndex = 0;
        IndicatorSizeCombo.SelectedIndex = Array.IndexOf(IndicatorSizes, s.IndicatorSize);
        if (IndicatorSizeCombo.SelectedIndex < 0) IndicatorSizeCombo.SelectedIndex = 0;
        MarginSlider.Value = s.IndicatorMargin;
        InitialStateCombo.SelectedIndex = Array.IndexOf(InitialStates, s.InitialState);
        if (InitialStateCombo.SelectedIndex < 0) InitialStateCombo.SelectedIndex = 0;
        StartWithWindowsBox.IsChecked = s.StartWithWindows;
    }

    private void HotkeyBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            _capturedHotkey = null;
            HotkeyBox.Text = _settings.Current.Hotkey.ToString();
            e.Handled = true;
            return;
        }

        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        {
            e.Handled = true;
            return;
        }

        if (key == Key.Enter)
        {
            if (_capturedHotkey == null && HotkeyBox.Text.Length > 0)
            {
                try
                {
                    _capturedHotkey = Hotkey.Parse(HotkeyBox.Text);
                    HotkeyBox.Text = _capturedHotkey.ToString();
                }
                catch (FormatException)
                {
                    HotkeyBox.Text = _settings.Current.Hotkey.ToString();
                }
            }
            e.Handled = true;
            return;
        }

        var mods = ToHotkeyModifiers(Keyboard.Modifiers);
        if (mods == HotkeyModifiers.None)
        {
            HotkeyBox.Text = "A modifier is required";
            e.Handled = true;
            return;
        }

        _capturedHotkey = Hotkey.FromVirtualKey(mods, KeyInterop.VirtualKeyFromKey(key));
        HotkeyBox.Text = _capturedHotkey.ToString();
        e.Handled = true;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosing(CancelEventArgs e)
    {
        Save();
        base.OnClosing(e);
    }

    private void Save()
    {
        var s = _settings.Current;
        if (_capturedHotkey != null && _capturedHotkey.HasModifiers)
            s.Hotkey = _capturedHotkey;

        s.SoundEnabled = SoundEnabledBox.IsChecked == true;
        s.Volume = (int)VolumeSlider.Value;
        s.VisualEnabled = VisualEnabledBox.IsChecked == true;
        s.PersistentIndicator = PersistentIndicatorBox.IsChecked == true;
        s.ControlMicrophone = ControlMicBox.IsChecked == true;
        s.NotificationDurationMs = int.TryParse(DurationBox.Text, out var d)
            ? Math.Clamp(d, 300, 10000)
            : 1000;
        s.Position = Positions[Math.Max(0, PositionCombo.SelectedIndex)];
        s.IndicatorSize = IndicatorSizes[Math.Max(0, IndicatorSizeCombo.SelectedIndex)];
        s.IndicatorMargin = (int)MarginSlider.Value;
        s.InitialState = InitialStates[Math.Max(0, InitialStateCombo.SelectedIndex)];

        var startWith = StartWithWindowsBox.IsChecked == true;
        s.StartWithWindows = startWith;
        StartWithWindows.SetEnabled(startWith);

        _settings.Save();
        _monitor.ApplySettings();
        _sound.Rebuild();
    }

    private static HotkeyModifiers ToHotkeyModifiers(ModifierKeys m)
    {
        var result = HotkeyModifiers.None;
        if ((m & ModifierKeys.Control) != 0) result |= HotkeyModifiers.Control;
        if ((m & ModifierKeys.Alt) != 0) result |= HotkeyModifiers.Alt;
        if ((m & ModifierKeys.Shift) != 0) result |= HotkeyModifiers.Shift;
        if ((m & ModifierKeys.Windows) != 0) result |= HotkeyModifiers.Win;
        return result;
    }
}
