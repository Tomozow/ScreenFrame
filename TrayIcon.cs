using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;

namespace ScreenFrame
{
    public class TrayIcon
    {
        private TaskbarIcon? _trayIcon;
        private MainWindow _mainWindow;

        public TrayIcon(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new TaskbarIcon
            {
                ToolTipText = "ScreenFrame - 右クリックまたはダブルクリックで設定",
                Visibility = Visibility.Visible
            };

            // アイコンを設定
            try
            {
                // リソースから埋め込まれたアイコンを読み込み
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("ScreenFrame.app.ico"))
                {
                    if (stream != null)
                    {
                        _trayIcon.Icon = new System.Drawing.Icon(stream);
                    }
                }
            }
            catch
            {
                // アイコンが読み込めない場合はデフォルトのまま
            }

            // コンテキストメニューを作成
            var contextMenu = new ContextMenu();
            
            var showSettingsItem = new MenuItem { Header = "設定モードON/OFF" };
            showSettingsItem.Click += (s, e) => ShowSettings();
            contextMenu.Items.Add(showSettingsItem);

            var toggleVisibilityItem = new MenuItem { Header = "枠の表示/非表示" };
            toggleVisibilityItem.Click += (s, e) => ToggleFrameVisibility();
            contextMenu.Items.Add(toggleVisibilityItem);

            contextMenu.Items.Add(new Separator()); // セパレーター

            var exitItem = new MenuItem { Header = "終了" };
            exitItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitItem);

            _trayIcon.ContextMenu = contextMenu;

            // ダブルクリックで設定画面を開く
            _trayIcon.TrayMouseDoubleClick += (s, e) => ShowSettings();
        }

        private void ShowSettings()
        {
            _mainWindow.ToggleSettingsMode();
        }

        private void ToggleFrameVisibility()
        {
            if (_mainWindow.Visibility == Visibility.Visible)
            {
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
            }
        }

        private void ExitApplication()
        {
            _trayIcon?.Dispose();
            System.Windows.Application.Current.Shutdown();
        }

        public void Dispose()
        {
            _trayIcon?.Dispose();
        }
    }
}