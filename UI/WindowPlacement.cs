using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace HotkeyStatusNotifier.UI;

public static class WindowPlacement
{
    public static void Apply(Window window, string position, int marginPx)
    {
        var screen = Screen.FromPoint(Control.MousePosition);
        var scale = VisualTreeHelper.GetDpi(window).DpiScaleX;

        var leftDip = (screen.WorkingArea.Left + marginPx) / scale;
        var topDip = (screen.WorkingArea.Top + marginPx) / scale;
        var rightDip = (screen.WorkingArea.Right - marginPx) / scale - window.Width;
        var bottomDip = (screen.WorkingArea.Bottom - marginPx) / scale - window.Height;
        var centerDip = ((screen.WorkingArea.Top + screen.WorkingArea.Bottom) / 2.0) / scale - window.Height / 2.0;

        switch (position)
        {
            case "BottomLeft":
                window.Left = leftDip; window.Top = bottomDip;
                break;
            case "TopRight":
                window.Left = rightDip; window.Top = topDip;
                break;
            case "TopLeft":
                window.Left = leftDip; window.Top = topDip;
                break;
            case "CenterRight":
                window.Left = rightDip; window.Top = centerDip;
                break;
            case "CenterLeft":
                window.Left = leftDip; window.Top = centerDip;
                break;
            default:
                window.Left = rightDip; window.Top = bottomDip;
                break;
        }
    }
}
