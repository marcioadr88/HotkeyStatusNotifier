using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HotkeyStatusNotifier.Core;
using HotkeyStatusNotifier.UI;

namespace HotkeyStatusNotifier.Services;

public sealed class TrayIconService : IDisposable
{
    private readonly SettingsService _settings;
    private readonly VoiceState _state;
    private readonly SoundService _sound;
    private readonly NotificationService _notification;
    private readonly HotkeyMonitor _monitor;
    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _mutedItem;
    private readonly ToolStripMenuItem _unmutedItem;
    private Icon? _mutedIcon;
    private Icon? _unmutedIcon;
    private IntPtr _mutedHicon;
    private IntPtr _unmutedHicon;
    private bool _disposed;

    public event EventHandler? ExitRequested;

    public TrayIconService(SettingsService settings, VoiceState state, SoundService sound,
        NotificationService notification, HotkeyMonitor monitor)
    {
        _settings = settings;
        _state = state;
        _sound = sound;
        _notification = notification;
        _monitor = monitor;

        (_mutedIcon, _mutedHicon) = BuildIcon(MicState.Muted);
        (_unmutedIcon, _unmutedHicon) = BuildIcon(MicState.Unmuted);

        _mutedItem = new ToolStripMenuItem("Muted");
        _mutedItem.Click += (_, _) => _state.ResetToMuted();

        _unmutedItem = new ToolStripMenuItem("Unmuted");
        _unmutedItem.Click += (_, _) => _state.Set(MicState.Unmuted);

        var test = new ToolStripMenuItem("Test notification");
        test.Click += (_, _) => Test();

        var reset = new ToolStripMenuItem("Reset to Muted");
        reset.Click += (_, _) => _state.ResetToMuted();

        var settingsItem = new ToolStripMenuItem("Settings");
        settingsItem.Click += (_, _) => OpenSettings();

        var exit = new ToolStripMenuItem("Exit");
        exit.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var menu = new ContextMenuStrip();
        menu.Items.Add(_mutedItem);
        menu.Items.Add(_unmutedItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(test);
        menu.Items.Add(reset);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(settingsItem);
        menu.Items.Add(exit);

        _icon = new NotifyIcon
        {
            Text = "HotkeyStatusNotifier",
            ContextMenuStrip = menu,
            Visible = true
        };
        _icon.DoubleClick += (_, _) => OpenSettings();

        SetState(_state.Current);
    }

    public void SetState(MicState state)
    {
        _icon.Icon = state == MicState.Muted ? _mutedIcon : _unmutedIcon;
        _icon.Text = state == MicState.Muted
            ? "HotkeyStatusNotifier - Mic Muted"
            : "HotkeyStatusNotifier - Mic Live";
        _mutedItem.Checked = state == MicState.Muted;
        _unmutedItem.Checked = state == MicState.Unmuted;
    }

    private void Test()
    {
        if (_settings.Current.SoundEnabled) _sound.Play(_state.Current);
        if (_settings.Current.VisualEnabled) _notification.Show(_state.Current);
    }

    private void OpenSettings()
    {
        _monitor.Pause();
        try
        {
            var window = new SettingsWindow(_settings, _monitor, _state, _sound);
            window.ShowDialog();
        }
        finally
        {
            _monitor.Resume();
            _monitor.ApplySettings();
            SetState(_state.Current);
        }
    }

    private static (Icon icon, IntPtr hicon) BuildIcon(MicState state)
    {
        var color = state == MicState.Muted
            ? Color.FromArgb(224, 66, 66)
            : Color.FromArgb(74, 190, 96);

        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            using var bg = new SolidBrush(color);
            g.FillEllipse(bg, 1, 1, 30, 30);

            using var fg = new SolidBrush(Color.White);
            g.FillRectangle(fg, 12, 6, 8, 14);
            g.FillEllipse(fg, 10, 20, 12, 5);

            using var pen = new Pen(Color.White, 2f);
            g.DrawArc(pen, 10, 16, 12, 12, 0, 180);
            g.DrawLine(pen, 16, 28, 16, 31);
            g.DrawLine(pen, 10, 31, 22, 31);
        }

        var hicon = bmp.GetHicon();
        return (Icon.FromHandle(hicon), hicon);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _icon.Visible = false;
        _icon.Dispose();
        if (_mutedHicon != IntPtr.Zero) DestroyIcon(_mutedHicon);
        if (_unmutedHicon != IntPtr.Zero) DestroyIcon(_unmutedHicon);
        _mutedIcon = null;
        _unmutedIcon = null;
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);
}
