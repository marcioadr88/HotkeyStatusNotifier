# Installation

## Requirements

- Windows 11
- **.NET 8 SDK** to build (or .NET 8 Desktop Runtime to run a prebuilt
  framework-dependent build). Download from https://dotnet.microsoft.com/download
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

The target PC needs the **.NET 8 Desktop Runtime** installed
(https://dotnet.microsoft.com/download/dotnet/8.0). Copy the whole output
folder and run the exe.

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
