# OBS Frame Guides

A lightweight Windows overlay that helps you keep your content inside the area you're recording in OBS. Two horizontal lines and two vertical lines stay above your desktop apps so you can see the edges of your shot.

## Download

**[Download the latest Windows app](https://github.com/glambershamble/obs-frame-guides/releases/latest)**

Download `OBS-Frame-Guides-Windows.zip` from the release, extract it, and open `OBS Frame Guides.exe`. No installer is needed.

## Features

- Drag each guide, or enter exact pixel positions.
- Lock the overlay so mouse clicks pass through to your apps.
- Change the line color and thickness.
- Include guides in screen captures, or exclude them from supported captures.
- Fit the guides to a monitor or reset to an inset on your main monitor.
- Save your guide positions and preferences automatically.
- Hide the controls in the system tray.

## How to use

1. Open the app and align the four lines with the desktop area your OBS source captures.
2. Enable **Lock guides (click through)** when you're ready to work.
3. Choose whether to **Include guides in screen capture**.
4. Click **To tray** to hide the controls before recording.

| Shortcut | Action |
| --- | --- |
| Ctrl + Alt + L | Lock or unlock guides |
| Ctrl + Alt + G | Show or hide guides |

Double-click the system tray icon to reopen the controls. Right-click it to exit. Closing the controls keeps the overlay running in the tray.

## OBS capture behavior

Use OBS **Display Capture** if you want the guides to appear in your recording. Window Capture and Game Capture may omit desktop overlays.

Capture exclusion uses Windows display affinity and depends on your Windows version and capture method. Check the OBS preview before recording. The controls window is a normal window; hide it with **To tray**.

The app does not change OBS scenes, source crops, or transforms. Align the guides with the screen area your OBS setup actually captures.

## Requirements and limitations

- Windows with .NET Framework 4.x, included on typical Windows 10/11 installations.
- Capture exclusion requires a supported capture method and Windows 10 version 2004 or newer / Windows 11.
- Works above ordinary desktop apps. Exclusive fullscreen games, elevated windows, or the Windows secure desktop may cover it. Use windowed or borderless mode for games.
- Guide coordinates are measured from the top-left of the entire virtual desktop, across all monitors.

## Build from source

Run `build.ps1` in PowerShell on Windows. It uses the .NET Framework C# compiler included with Windows and produces `OBS Frame Guides.exe` beside the source. No third-party packages are required.

Settings are stored in `%LOCALAPPDATA%\OBSFrameGuides\settings.txt`.

This is an independent utility and is not affiliated with OBS Studio.
