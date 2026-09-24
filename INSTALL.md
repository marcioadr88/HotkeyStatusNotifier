# Installation

## Download a release (recommended)

1. Download `HotkeyStatusNotifier.exe` from the
   [latest release](https://github.com/marcioadr88/HotkeyStatusNotifier/releases/latest)
   (see the **Assets** section of the release page).
2. Install the **.NET 8 Desktop Runtime (x64)** on the PC where you want to
   run the app: https://dotnet.microsoft.com/download/dotnet/8.0 — run the
   downloaded installer with the default options. The release exe is a
   compact, **framework-dependent** build: the runtime is required, the
   **.NET SDK is not** needed to run it.
3. Double-click `HotkeyStatusNotifier.exe`. A tray icon appears (green mic =
   Unmuted). Press `Ctrl+Alt+M` to test the toggle. See
   [First run](#first-run) below.

## Requirements

- Windows 11
- **.NET 8 Desktop Runtime (x64)** to run the release exe (framework-dependent)
  — https://dotnet.microsoft.com/download/dotnet/8.0
- **.NET 8 SDK** to build from source (developer machines only)
- No other dependencies. No NuGet packages.

## Build (developer machine)

```powershell
cd HotkeyStatusNotifier
dotnet --version        # 8.x
dotnet build
```

The exe is created at `bin\Debug\net8.0-windows\HotkeyStatusNotifier.exe`.
Build a release build with `dotnet build -c Release`.

## Option A — framework-dependent (needs runtime on target PC)

The target PC needs the **.NET 8 Desktop Runtime (x64)** installed
(https://dotnet.microsoft.com/download/dotnet/8.0). Copy the whole output
folder and run the exe. (The GitHub Release exe is this kind of build — a
single-file, framework-dependent exe.)

## Option B — self-contained single file (no runtime needed)

On the developer machine:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish\win-x64
```

Copy `publish\win-x64\HotkeyStatusNotifier.exe` to the target PC. It runs
without installing anything, at the cost of a larger file (~80–150 MB) and a
possible SmartScreen warning because the exe is unsigned.

## Deploy

Put the app in any folder you can write to (e.g. `C:\Tools\HotkeyStatusNotifier`
or under your user profile). It needs no admin rights.

## First run

Double-click the exe. A tray icon appears (green mic = Unmuted). Press
`Ctrl+Alt+M` to test the toggle. Use the tray menu → `Settings` to configure
everything and to enable **Start with Windows**.

## Uninstall

- Tray menu → `Settings` → untick **Start with Windows**.
- Exit the app from the tray menu.
- Delete the app folder. Optionally delete `%APPDATA%\HotkeyStatusNotifier`.
