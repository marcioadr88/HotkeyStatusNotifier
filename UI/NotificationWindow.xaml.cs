using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace HotkeyStatusNotifier.UI;

public partial class NotificationWindow : Window
{
    private const int WsExTransparent = 0x00000020;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;
    private const int GWL_EXSTYLE = -20;
    private const int FadeInMs = 120;
    private const int FadeOutMs = 180;

    private readonly int _durationMs;
    private readonly string _position;
    private readonly int _margin;

    public NotificationWindow(string text, int durationMs, string position, int margin)
    {
        InitializeComponent();
        _durationMs = Math.Max(300, durationMs);
        _position = position;
        _margin = margin;

        var space = text.IndexOf(' ');
        IconText.Text = space > 0 ? text[..space] : text;
        StateText.Text = space > 0 ? text[(space + 1)..] : "";

        Loaded += OnLoaded;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var style = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, style | WsExTransparent | WsExNoActivate | WsExToolWindow);
        }
        catch
        {
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            PositionOnScreen();
        }
        catch
        {
        }
        BeginFade();
    }

    private void PositionOnScreen()
    {
        WindowPlacement.Apply(this, _position, _margin);
    }

    private void BeginFade()
    {
        try
        {
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(FadeInMs));
            Root.BeginAnimation(OpacityProperty, fadeIn);
        }
        catch
        {
            Root.Opacity = 1;
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_durationMs + FadeInMs) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            try
            {
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(FadeOutMs));
                fadeOut.Completed += (_, _) => Close();
                Root.BeginAnimation(OpacityProperty, fadeOut);
            }
            catch
            {
                Close();
            }
        };
        timer.Start();
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", CharSet = CharSet.Unicode)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", CharSet = CharSet.Unicode)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
