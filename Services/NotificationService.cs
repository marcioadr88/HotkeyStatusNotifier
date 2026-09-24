using HotkeyStatusNotifier.Core;
using HotkeyStatusNotifier.UI;

namespace HotkeyStatusNotifier.Services;

public sealed class NotificationService
{
    private readonly SettingsService _settings;

    public NotificationService(SettingsService settings) => _settings = settings;

    public void Show(MicState state)
    {
        var text = state == MicState.Muted ? "\U0001F507 MIC MUTED" : "\U0001F399 MIC LIVE";
        var s = _settings.Current;
        var window = new NotificationWindow(text, s.NotificationDurationMs, s.Position, s.IndicatorMargin);
        window.Show();
    }
}
