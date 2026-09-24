# HotkeyStatusNotifier

Small, generic Windows 11 tray app that gives **sound + visual feedback** when you
press a global hotkey (default `Ctrl+Alt+M`). It toggles a local `Muted` /
`Unmuted` state and shows a brief on-screen indicator.

It is **not** tied to any game or application. It does not read game memory,
inject DLLs, send keys, or consume the hotkey — the key combination still
reaches the app that has focus (e.g. a game's push-to-talk toggle).

## Download

Grab the latest release from
[GitHub Releases](https://github.com/marcioadr88/HotkeyStatusNotifier/releases/latest).

The release download (`HotkeyStatusNotifier.exe`) is a compact,
**framework-dependent** build: it needs the **.NET 8 Desktop Runtime (x64)**
installed on the target PC — the SDK is **not** required to run it. See
[INSTALL.md](INSTALL.md) for the runtime download link and setup.

## What it does

- Observes a global hotkey without blocking it (Windows **Raw Input**).
- Toggles `Muted` / `Unmuted` locally; optionally **mutes/unmutes the actual
  Windows input device** (standard audio endpoint API) when "Control
  microphone" is enabled.
- Plays a distinct short tone on each toggle (two-note blips: muted descends,
  live ascends).
- Shows a small, click-through, non-focus-stealing notification for ~1s
  (`🔇 MIC MUTED` / `🎙 MIC LIVE`), or an optional always-visible mic widget
  (red/green) that replaces it.
- Runs in the system tray with a state-colored icon and menu.

## Safety / anti-cheat summary

- Detection uses Raw Input (`RegisterRawInputDevices` + `RIDEV_INPUTSINK`) —
  pure observation. The system delivers input to the focused app and to
  subscribers; this app cannot block, redirect, or re-send anything.
- No keyboard hooks (`SetWindowsHookEx`), no DLL injection, no memory reads
  (`ReadProcessMemory`), no synthetic input (`SendInput`, `keybd_event`).
- No interaction with any game process. It never touches the game.
- The optional mic control only sets a system audio setting (the default input
  device's mute state — same as changing the volume); it does not touch game
  processes or memory.
- Residual risk: a background keyboard listener can be *observed* by some
  kernel anti-cheat regardless of API. Raw Input is the mechanism accessibility
  tools use, not a cheat pattern, but no API choice gives immunity. See
  `USAGE.md` for game-specific notes (fullscreen behavior).

## Docs

- [Installation](INSTALL.md)
- [Usage](USAGE.md)
- [Troubleshooting](TROUBLESHOOTING.md)

## Build (short version)

Requires .NET 8 SDK on Windows 11 (end users don't need the SDK — download the
release exe instead). See `INSTALL.md` for details and the self-contained
(no-runtime) option.

```
dotnet build
```

Run: `bin\Debug\net8.0-windows\HotkeyStatusNotifier.exe`
