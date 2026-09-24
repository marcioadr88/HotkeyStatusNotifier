using System;
using System.IO;
using System.Media;
using System.Text;
using HotkeyStatusNotifier.Core;

namespace HotkeyStatusNotifier.Services;

public sealed class SoundService
{
    private const int SampleRate = 44100;

    private readonly SettingsService _settings;
    private SoundPlayer? _mutedPlayer;
    private SoundPlayer? _unmutedPlayer;

    public SoundService(SettingsService settings)
    {
        _settings = settings;
        Rebuild();
    }

    public void Rebuild()
    {
        DisposePlayers();
        var volume = _settings.Current.Volume / 100.0;
        _mutedPlayer = new SoundPlayer(BuildWavStream(new[] { 392.0, 330.0 }, 0.075, volume));
        _unmutedPlayer = new SoundPlayer(BuildWavStream(new[] { 392.0, 523.0 }, 0.075, volume));
    }

    public void Play(MicState state)
    {
        var player = state == MicState.Muted ? _mutedPlayer : _unmutedPlayer;
        if (player == null) return;
        try
        {
            player.Play();
        }
        catch
        {
        }
    }

    private static Stream BuildWavStream(double[] freqs, double perNote, double volume)
    {
        var perNoteSamples = (int)(SampleRate * perNote);
        var samples = new short[freqs.Length * perNoteSamples];
        for (var f = 0; f < freqs.Length; f++)
        {
            for (var i = 0; i < perNoteSamples; i++)
            {
                var t = (double)i / SampleRate;
                var attack = Math.Min(1.0, t / 0.008);
                var release = Math.Min(1.0, (perNote - t) / 0.02);
                var envelope = attack * release;
                var v = Math.Sin(2 * Math.PI * freqs[f] * t) * envelope * volume;
                samples[f * perNoteSamples + i] = (short)(Math.Clamp(v, -1.0, 1.0) * short.MaxValue);
            }
        }
        return BuildWav(samples, SampleRate);
    }

    private static MemoryStream BuildWav(short[] samples, int sampleRate)
    {
        var dataLen = samples.Length * 2;
        var ms = new MemoryStream();
        using (var w = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            w.Write(Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + dataLen);
            w.Write(Encoding.ASCII.GetBytes("WAVE"));
            w.Write(Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);
            w.Write((short)1);
            w.Write((short)1);
            w.Write(sampleRate);
            w.Write(sampleRate * 2);
            w.Write((short)2);
            w.Write((short)16);
            w.Write(Encoding.ASCII.GetBytes("data"));
            w.Write(dataLen);
            foreach (var s in samples) w.Write(s);
        }
        ms.Position = 0;
        return ms;
    }

    private void DisposePlayers()
    {
        _mutedPlayer?.Dispose();
        _unmutedPlayer?.Dispose();
        _mutedPlayer = null;
        _unmutedPlayer = null;
    }
}
