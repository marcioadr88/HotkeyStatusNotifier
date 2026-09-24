using System;

namespace HotkeyStatusNotifier.Core;

public enum MicState
{
    Muted,
    Unmuted
}

public sealed class VoiceState
{
    public VoiceState(MicState initial = MicState.Unmuted)
    {
        Current = initial;
    }

    public MicState Current { get; private set; }

    public event EventHandler<MicState>? Changed;

    public void Toggle() => Set(Current == MicState.Muted ? MicState.Unmuted : MicState.Muted);

    public void Set(MicState value)
    {
        if (Current == value) return;
        Current = value;
        Changed?.Invoke(this, value);
    }

    public void ResetToMuted() => Set(MicState.Muted);
}
