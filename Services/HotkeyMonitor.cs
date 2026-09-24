using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using HotkeyStatusNotifier.Core;

namespace HotkeyStatusNotifier.Services;

public sealed class HotkeyMonitor : IDisposable
{
    private const int WM_INPUT = 0x00FF;
    private const uint RIDEV_INPUTSINK = 0x00000100;
    private const ushort HID_USAGE_PAGE_GENERIC = 0x01;
    private const ushort HID_USAGE_KEYBOARD = 0x06;
    private const uint RIM_TYPEKEYBOARD = 1;
    private const uint RID_INPUT = 0x10000003;
    private const uint RI_KEY_BREAK = 1;
    private const uint RI_KEY_E0 = 2;
    private const uint RI_KEY_E1 = 4;

    private readonly SettingsService _settings;
    private HwndSource? _source;
    private bool _paused;
    private bool _mainDown;
    private bool _disposed;

    public event EventHandler? HotkeyPressed;

    public HotkeyMonitor(SettingsService settings)
    {
        _settings = settings;
        _source = new HwndSource(new HwndSourceParameters("HotkeyStatusNotifierInputSink")
        {
            WindowStyle = 0,
            ExtendedWindowStyle = 0
        });
        _source.AddHook(WndProc);
        Register();
    }

    public void ApplySettings() => Register();

    public void Pause() => _paused = true;

    public void Resume() => _paused = false;

    private void Register()
    {
        if (_source == null) return;
        var device = new RAWINPUTDEVICE
        {
            UsagePage = HID_USAGE_PAGE_GENERIC,
            Usage = HID_USAGE_KEYBOARD,
            Flags = RIDEV_INPUTSINK,
            hwndTarget = _source.Handle
        };
        if (!RegisterRawInputDevices(ref device, 1, (uint)Marshal.SizeOf<RAWINPUTDEVICE>()))
        {
            try
            {
                System.IO.File.AppendAllText(SettingsService.ErrorLogPath,
                    $"[{DateTime.Now:O}] RegisterRawInputDevices failed, Win32 error {Marshal.GetLastWin32Error()}\n");
            }
            catch
            {
            }
        }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_INPUT)
        {
            handled = true;
            HandleRawInput(lParam);
            return IntPtr.Zero;
        }
        return IntPtr.Zero;
    }

    private void HandleRawInput(IntPtr lParam)
    {
        if (_paused) return;

        var size = 0u;
        GetRawInputData(lParam, RID_INPUT, IntPtr.Zero, ref size, (uint)Marshal.SizeOf<RAWINPUTHEADER>());
        if (size == 0) return;

        var buffer = Marshal.AllocHGlobal((int)size);
        try
        {
            var got = size;
            if (GetRawInputData(lParam, RID_INPUT, buffer, ref got, (uint)Marshal.SizeOf<RAWINPUTHEADER>()) != size)
                return;
            var raw = Marshal.PtrToStructure<RAWINPUT>(buffer);
            if (raw.header.dwType != RIM_TYPEKEYBOARD) return;
            ProcessKeyboard(raw.keyboard);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private void ProcessKeyboard(RAWKEYBOARD kb)
    {
        var hotkey = _settings.Current.Hotkey;
        var isMain =
            (hotkey.ScanCode != 0 && kb.MakeCode == hotkey.ScanCode) ||
            (hotkey.ScanCode == 0 && kb.VKey == hotkey.VirtualKey);
        if (!isMain) return;
        if ((kb.Flags & (RI_KEY_E0 | RI_KEY_E1)) != (hotkey.ExtendedFlags & (RI_KEY_E0 | RI_KEY_E1))) return;

        var isBreak = (kb.Flags & RI_KEY_BREAK) != 0;
        if (isBreak)
        {
            _mainDown = false;
            return;
        }

        if (_mainDown) return;
        _mainDown = true;

        if (ModifiersDown(hotkey.Modifiers))
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
    }

    private static bool ModifiersDown(HotkeyModifiers mods)
    {
        if ((mods & HotkeyModifiers.Control) != 0 && (GetAsyncKeyState(0x11) & 0x8000) == 0) return false;
        if ((mods & HotkeyModifiers.Alt) != 0 && (GetAsyncKeyState(0x12) & 0x8000) == 0) return false;
        if ((mods & HotkeyModifiers.Shift) != 0 && (GetAsyncKeyState(0x10) & 0x8000) == 0) return false;
        if ((mods & HotkeyModifiers.Win) != 0 &&
            (GetAsyncKeyState(0x5B) & 0x8000) == 0 &&
            (GetAsyncKeyState(0x5C) & 0x8000) == 0) return false;
        return true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _source?.Dispose();
        _source = null;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTDEVICE
    {
        public ushort UsagePage;
        public ushort Usage;
        public uint Flags;
        public IntPtr hwndTarget;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWKEYBOARD
    {
        public ushort MakeCode;
        public ushort Flags;
        public ushort Reserved;
        public ushort VKey;
        public uint Message;
        public uint ExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RAWINPUT
    {
        public RAWINPUTHEADER header;
        public RAWKEYBOARD keyboard;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterRawInputDevices(ref RAWINPUTDEVICE pRawInputDevices, uint uiNumDevices, uint cbSize);

    [DllImport("user32.dll")]
    private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
