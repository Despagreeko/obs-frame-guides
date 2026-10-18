OBS FRAME GUIDES

Open OBS Frame Guides.exe. No installation is needed.

Drag the four lines to match the screen area you want to keep inside your OBS
shot. The thicker grips help you grab a line. For exact placement, enter the
Left, Right, Top and Bottom positions in the controls. Positions are pixels
from the top-left of the entire desktop, including all monitors.

Turn on "Lock guides (click through)" when ready. Your mouse then works
normally in the apps underneath. The guide positions, color, width and lock
state are saved automatically.

SHORTCUTS
Ctrl + Alt + L: Lock / unlock
Ctrl + Alt + G: Show / hide

"Fit monitor" places the guides at a monitor's edges.
"Reset inset" gives you a starting rectangle on your main monitor.
"To tray" or closing the controls hides the controls but keeps the guides.
Double-click the tray icon to reopen controls. Right-click it to exit.
If Windows has hidden the icon, look in the system tray's overflow menu.

OBS
Turn on "Include guides in screen capture" to let the guides appear in OBS
Display Capture or other desktop captures. Turn it off to exclude them again.
This preference is saved. Capturing another application's window or using Game
Capture may omit desktop overlays; use Display Capture to capture the guides.

By default, the guide window asks Windows to exclude it from captures. This works
with supported capture methods on Windows 10 version 2004 and newer, and
Windows 11. Check your OBS preview before recording: capture methods differ.
The control window is a normal window, so hide it with "To tray" before filming.
The app does not read or change OBS settings. Align the lines with the desktop
area your OBS source actually captures; scene crops and transforms are separate.

The overlay stays above ordinary desktop apps. Exclusive fullscreen games,
the Windows secure desktop, and some elevated windows may cover it. Use
windowed or borderless mode for games.

REQUIREMENTS
Windows with .NET Framework 4.x (included on typical Windows 10/11 PCs).

SOURCE
FrameGuides.cs is included. Run build.ps1 to rebuild with the Windows compiler.
Settings are stored in %LOCALAPPDATA%\OBSFrameGuides\settings.txt.
