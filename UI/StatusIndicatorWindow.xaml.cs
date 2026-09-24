using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using HotkeyStatusNotifier.Core;

namespace HotkeyStatusNotifier.UI;

public partial class StatusIndicatorWindow : Window
{
    private const int WsExTransparent = 0x00000020;
    private const int WsExNoActivate = 0x08000000;
    private const int WsExToolWindow = 0x00000080;
    private const int GWL_EXSTYLE = -20;

    private static readonly Brush MutedBrush = new SolidColorBrush(Color.FromRgb(224, 66, 66));
    private static readonly Brush LiveBrush = new SolidColorBrush(Color.FromRgb(74, 190, 96));

    private string _position = "BottomRight";
    private int _margin = 8;

    public StatusIndicatorWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public void SetState(MicState state) => Bg.Fill = state == MicState.Muted ? MutedBrush : LiveBrush;

    public void ApplySettings(Settings settings)
    {
        _position = settings.Position;
        _margin = settings.IndicatorMargin;
        IconBox.Width = IconBox.Height = SizeFor(settings.IndicatorSize);
        if (settings.PersistentIndicator)
        {
            if (!IsVisible) Show();
            Reposition();
        }
        else if (IsVisible)
        {
            Hide();
        }
    }

    private void Reposition()
    {
        try
        {
            WindowPlacement.Apply(this, _position, _margin);
        }
        catch
        {
        }
        Dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                WindowPlacement.Apply(this, _position, _margin);
            }
            catch
            {
            }
        }), DispatcherPriority.Loaded);
    }

    private static double SizeFor(string size) => size switch
    {
        "Medium" => 32,
        "Small" => 24,
        _ => 44
    };

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
            WindowPlacement.Apply(this, _position, _margin);
        }
        catch
        {
        }
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW", CharSet = CharSet.Unicode)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", CharSet = CharSet.Unicode)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
}
