using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using HotkeyStatusNotifier.Core;

namespace HotkeyStatusNotifier;

public partial class App : System.Windows.Application
{
    static App()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogError(e.Exception);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            LogError(ex);
    }

    private static void LogError(Exception ex)
    {
        try
        {
            Directory.CreateDirectory(SettingsService.DataDir);
            File.AppendAllText(SettingsService.ErrorLogPath, $"[{DateTime.Now:O}] {ex}\n\n");
        }
        catch
        {
        }
    }
}
