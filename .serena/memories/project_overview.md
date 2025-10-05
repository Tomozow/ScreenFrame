# ScreenFrame
- Purpose: Windows 10/11 WPF overlay app that draws a customizable screen frame (position, size, color, thickness) for streaming/recording.
- Tech stack: .NET 6.0 (WPF, WinExe), C#, uses `Hardcodet.NotifyIcon.Wpf` for tray icon and `System.Text.Json` for settings persistence.
- Key components:
  * `MainWindow.xaml(.cs)`: transparent, click-through overlay window with drag/resize handles, settings toggling, input hooks, and persistence glue.
  * `TrayIcon.cs`: sets up tray menu (toggle settings mode, show/hide frame, exit) and loads embedded icon.
  * `Settings.cs`: `AppSettings` model persisted under `%APPDATA%\ScreenFrame\settings.json` via `SettingsManager`.
  * `App.xaml(.cs)`: WPF application startup shell.
  * `create_icon.py` scripts: utility to generate icon assets.
- Misc: icons stored in repo (`app.ico`, `icon_*.png`), release builds under `release`/`release_full` folders.