# Troubleshooting

Logs: `%APPDATA%\HotkeyStatusNotifier\error.log` (created on errors).
Settings: `%APPDATA%\HotkeyStatusNotifier\settings.json`.

## The hotkey does nothing

1. Is the app running? Check the tray; check `tasklist | findstr HotkeyStatusNotifier`.
2. Is the window/editor you are typing in normal? Hotkeys never fire during a
   UAC prompt, the lock screen, or Ctrl+Alt+Del (secure desktop).
3. Is the game in **exclusive fullscreen**? Detection should still work (Raw
   Input is not blocked by exclusive mode), but the on-screen feedback is not
   composited over it. If the hotkey itself fails in-game, switch the game to
   borderless-windowed.
4. Is the game running **as administrator**? Elevated processes isolate their
   input (UIPI). Run the game normally.
5. Did you change the hotkey? Re-check in Settings. Press the combo and confirm
   the capture field shows exactly what you press.
6. Keyboard layout: matching uses the physical key you captured. If you changed
   layout, re-capture the hotkey.
7. Sticky Keys / Filter Keys (Ease of Access) can interfere; try disabling them.
8. Check `error.log` for a `RegisterRawInputDevices failed` line.

## No sound

1. Is **Sound enabled** in Settings? Is volume > 0?
2. Is the game using the audio endpoint exclusively (some exclusive-fullscreen
   setups)? Switch the game to borderless.
3. The tone is short by design (two quick notes, ~150 ms total). Try `Test notification` to verify.
4. Windows volume mixer: this app uses legacy winmm output; it may not show its
   own mixer slider. The configured volume is applied to the samples directly.

## Microphone is not muted / unmuted

1. Is **Control microphone** enabled in Settings? (It is on by default.)
2. Is there a default input device? Check System → Sound → Input. If there is
   none, the toggle falls back to local-only state.
3. Verify it visually: with the app running, press the hotkey and check the mic
   mute toggle in System → Sound → Input (or the mic icon in the volume flyout).
4. Some apps open the mic in exclusive/WASAPI mode and may not reflect the
   endpoint mute; the game must use the system default input device.
5. If the mute never takes effect, check `error.log` — COM/audio errors are
   swallowed and logged there.

## Notification does not appear

1. Is **Visual indicator enabled** in Settings?
2. Duration too short (< 300 ms is clamped to 300 ms).
3. Exclusive fullscreen: the overlay is not composited over it. Use borderless.
4. Wrong monitor? The indicator appears on the monitor where the mouse cursor
   is. Move the cursor to the gaming monitor and toggle again.

## Notification steals clicks / passes clicks

It is click-through by design (`WS_EX_TRANSPARENT` + `WS_EX_NOACTIVATE`). If a
click is swallowed during the ~1s it is visible, that is the default; move the
cursor away.

## Does not start with Windows

1. Re-tick **Start with Windows** in Settings after moving the exe.
2. Verify the registry value is a **quoted** path:
   `reg query HKCU\Software\Microsoft\Windows\CurrentVersion\Run /v HotkeyStatusNotifier`
   It must look like `"C:\path\to\HotkeyStatusNotifier.exe"`.
3. Some security tools block Run-key autostart; allow it.

## It ran before, now starts with defaults

`settings.json` was corrupt or deleted. Delete the file and reconfigure.

## Two instances

Only one instance runs; launching again exits silently.

## Reset everything

- Tray → `Exit`.
- Delete `%APPDATA%\HotkeyStatusNotifier`.
- Tray menu Settings → untick **Start with Windows** (or remove the registry
  value manually with `reg delete ... /v HotkeyStatusNotifier /f`).

## I want to verify it is not touching the game

Process Explorer / Process Monitor will show: no `OpenProcess` on game
processes, no `ReadProcessMemory`, no `WriteProcessMemory`, no `SendInput`,
no `SetWindowsHookEx`. It only registers a Raw Input device (keyboard) and
reads `GetAsyncKeyState` to confirm modifiers.
