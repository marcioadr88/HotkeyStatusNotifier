using System;
using System.Threading;
using HotkeyStatusNotifier.Core;
using HotkeyStatusNotifier.Services;
using HotkeyStatusNotifier.UI;

namespace HotkeyStatusNotifier;

public static class Program
{
    private const string MutexName = "Local\\HotkeyStatusNotifier.SingleInstance";

    [STAThread]
    public static void Main()
    {
        System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.PerMonitorV2);

        using var mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
            return;

        var settings = new SettingsService();
        settings.Load();

        if (settings.Current.StartWithWindows && !StartWithWindows.IsEnabled())
            StartWithWindows.SetEnabled(true);

        var mic = new MicController();

        MicState initial;
        if (settings.Current.ControlMicrophone && mic.TryGetMuted(out var muted))
            initial = muted ? MicState.Muted : MicState.Unmuted;
        else
            initial = settings.Current.InitialState == "Muted" ? MicState.Muted : MicState.Unmuted;

        var state = new VoiceState(initial);
        var sound = new SoundService(settings);
        var notification = new NotificationService(settings);

        var app = new App();
        app.InitializeComponent();

        var indicator = new StatusIndicatorWindow();

        var monitor = new HotkeyMonitor(settings);
        var tray = new TrayIconService(settings, state, sound, notification, monitor);

        state.Changed += (_, m) =>
        {
            tray.SetState(m);
            indicator.SetState(m);
            if (settings.Current.ControlMicrophone) mic.TrySetMuted(m == MicState.Muted);
            if (settings.Current.SoundEnabled) sound.Play(m);
            if (settings.Current.VisualEnabled && !settings.Current.PersistentIndicator) notification.Show(m);
        };

        monitor.HotkeyPressed += (_, _) => state.Toggle();
        tray.ExitRequested += (_, _) => app.Shutdown();

        settings.SettingsChanged += (_, _) => indicator.ApplySettings(settings.Current);

        tray.SetState(state.Current);
        indicator.SetState(state.Current);
        indicator.ApplySettings(settings.Current);
        app.Run();
    }
}
