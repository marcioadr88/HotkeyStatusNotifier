# Usage

## Start / stop

- **Start:** run the exe. A tray icon appears. No main window.
- **Stop:** tray menu → `Exit`.
- **Start with Windows:** Settings → tick "Start with Windows". Uses a per-user
  registry entry (`HKCU\...\CurrentVersion\Run`), no admin needed.

## States

- `Unmuted` — green mic icon.
- `Muted` — red mic icon.
- The hotkey toggles between them. With **Control microphone** off, the app
  tracks a **local** state only. Startup state is `Unmuted` by default; set
  "Start with state" in Settings to start as `Muted` instead.

## Controlling the microphone

- When **Control microphone** is on (default), each toggle also
  mutes/unmutes the actual Windows default input device — the same state shown
  in System → Sound → Input. This covers games that have no push-to-talk or
  mute key: the app becomes the mute control.
- The app syncs to the real device at startup; while the option is on, the app
  is the authoritative controller of the mic.
- If there is no default input device, the toggle falls back to local-only
  state (the feedback still plays).
- Disable it if you only want the status indicator and another app handles the
  mute.

## Feedback

- **Sound:** muted = short descending blip, live = short ascending blip. Volume in Settings
  (0–100). Can be disabled.
- **Visual:** small translucent card, bottom-right by default
  (`🔇 MIC MUTED` / `🎙 MIC LIVE`), shown ~1s, auto-closes. It never takes
  focus and clicks pass through it. Position, duration and on/off in Settings.
- **Always-visible indicator:** optionally keep a small round colored mic icon
  on screen at all times (red = muted, green = live) instead of the brief
  notification. Enable "Always-visible indicator" in Settings; when on, the
  brief notification is skipped. The widget is always-on-top and click-through,
  and its size (Large / Medium / Small) and distance from the screen edge
  (0 = flush, negative values overlap the edge) are configurable.

## Tray menu

| Item | Action |
|---|---|
| `Muted` / `Unmuted` | Show/set the current state (the active one is checked) |
| `Test notification` | Play the current state's sound + notification |
| `Reset to Muted` | Force state to Muted |
| `Settings` | Open settings window |
| `Exit` | Quit the app |

## Changing the hotkey

1. Tray → `Settings`.
2. Click the hotkey field, then press the combination (e.g. `Ctrl+Alt+F5`).
   Esc cancels. You can also type it (e.g. `Ctrl+Alt+M`) and press Enter.
3. Close the window. The change applies immediately.

## Playing a game

- The hotkey is not consumed: if the game has its own toggle for the same
  combination, it also receives it. If it does not (many games don't), enable
  **Control microphone** so the app mutes the real input device.
- **Exclusive fullscreen:** hotkey detection uses Raw Input and is expected to
  keep working, but the on-screen indicator/notification is **not** composited
  over exclusive fullscreen (Windows limitation) — feedback is sound-only.
  Use **borderless-windowed** to also get the visual feedback.
- **Running a game as administrator:** a non-elevated app cannot observe input
  of an elevated process (UIPI). Do not elevate the game; run it normally. Do
  not run this app as administrator.
- **AltGr:** on Spanish/Portuguese layouts AltGr equals Ctrl+Alt, so
  `AltGr+M` will also trigger. Avoid hotkeys whose key has a common AltGr
  symbol if this bothers you.
- The hotkey does not fire during UAC prompts, the lock screen, or
  Ctrl+Alt+Del.
