using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace ScreenFrame
{
    public partial class MainWindow : Window
    {
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private const double MinimumWindowSize = 50.0;
        private const double DefaultBorderThickness = 3.0;

        private bool _isClickThrough = true;
        private TrayIcon _trayIcon;
        private AppSettings _settings = new AppSettings();
        
        // ドラッグ関連
        private bool _isDragging = false;
        private POINT _dragStartScreenPoint;
        private Point _windowStartPoint;
        
        // リサイズ関連
        private bool _isResizing = false;
        private string _resizeMode = "";
        private Point _resizeStartPoint;
        private Rect _originalBounds;

        public MainWindow()
        {
            InitializeComponent();
            
            // 設定を読み込み
            LoadSettings();
            
            Loaded += OnLoaded;
            
            // ESCキーのみ有効（設定モード終了用）
            PreviewKeyDown += OnPreviewKeyDown;
            
            // システムトレイアイコンを初期化
            _trayIcon = new TrayIcon(this);
            
            // ドラッグとリサイズのイベント設定
            InitializeDragAndResize();
            
            // 右クリックメニューの設定
            InitializeContextMenu();
            
            // 安全性のためのグローバルマウスイベント
            MouseLeave += OnMouseLeave;
            
            // アプリケーション終了時に設定を保存
            Closing += OnClosing;
        }

        private void InitializeDragAndResize()
        {
            // メインボーダーでドラッグ移動
            MainBorder.MouseLeftButtonDown += OnMainBorderMouseDown;
            MainBorder.MouseMove += OnMainBorderMouseMove;
            MainBorder.MouseLeftButtonUp += OnMainBorderMouseUp;
            
            // リサイズハンドルの設定
            SetupResizeHandle(TopLeftHandle, "TopLeft");
            SetupResizeHandle(TopRightHandle, "TopRight");
            SetupResizeHandle(BottomLeftHandle, "BottomLeft");
            SetupResizeHandle(BottomRightHandle, "BottomRight");
            SetupResizeHandle(TopHandle, "Top");
            SetupResizeHandle(BottomHandle, "Bottom");
            SetupResizeHandle(LeftHandle, "Left");
            SetupResizeHandle(RightHandle, "Right");
            
            // 辺ハンドルの位置を調整
            UpdateEdgeHandles();
            SizeChanged += (s, e) => UpdateEdgeHandles();
        }
        
        private void InitializeContextMenu()
        {
            var contextMenu = new ContextMenu();
            
            // 色選択メニュー
            var colorMenu = new MenuItem { Header = "色を変更" };
            
            // カスタム色選択
            var customColorItem = new MenuItem { Header = "カスタム色..." };
            customColorItem.Click += (s, e) => ShowColorDialog();
            colorMenu.Items.Add(customColorItem);
            
            colorMenu.Items.Add(new Separator());
            
            // プリセット色
            var colors = new[]
            {
                new { Name = "赤", Color = Colors.Red },
                new { Name = "緑", Color = Colors.Lime },
                new { Name = "青", Color = Colors.Blue },
                new { Name = "黄", Color = Colors.Yellow },
                new { Name = "マゼンタ", Color = Colors.Magenta },
                new { Name = "シアン", Color = Colors.Cyan },
                new { Name = "白", Color = Colors.White },
                new { Name = "黒", Color = Colors.Black },
                new { Name = "オレンジ", Color = Colors.Orange }
            };
            
            foreach (var colorInfo in colors)
            {
                var menuItem = new MenuItem { Header = colorInfo.Name };
                menuItem.Click += (s, e) => ChangeFrameColor(colorInfo.Color);
                colorMenu.Items.Add(menuItem);
            }
            
            contextMenu.Items.Add(colorMenu);
            
            // 枠の太さメニュー
            var thicknessMenu = new MenuItem { Header = "枠の太さ" };
            
            for (int thickness = 1; thickness <= 10; thickness++)
            {
                var menuItem = new MenuItem { Header = $"{thickness}px" };
                var t = thickness; // クロージャ対策
                menuItem.Click += (s, e) => ChangeFrameThickness(t);
                thicknessMenu.Items.Add(menuItem);
            }
            
            contextMenu.Items.Add(thicknessMenu);
            
            contextMenu.Items.Add(new Separator());
            
            // プリセットメニュー
            var presetMenu = new MenuItem { Header = "プリセットサイズ" };
            
            var presets = new[]
            {
                new { Name = "720p (1280×720)", Width = 1280.0, Height = 720.0 },
                new { Name = "1080p (1920×1080)", Width = 1920.0, Height = 1080.0 },
                new { Name = "4:3 (800×600)", Width = 800.0, Height = 600.0 },
                new { Name = "16:9 (1600×900)", Width = 1600.0, Height = 900.0 }
            };
            
            foreach (var preset in presets)
            {
                var menuItem = new MenuItem { Header = preset.Name };
                menuItem.Click += (s, e) => ResizeToPreset(preset.Width, preset.Height);
                presetMenu.Items.Add(menuItem);
            }
            
            contextMenu.Items.Add(presetMenu);
            
            MainBorder.ContextMenu = contextMenu;
        }
        
        private void ShowColorDialog()
        {
            using (var colorDialog = new WinForms.ColorDialog())
            {
                // 現在の色を初期値として設定
                var currentBrush = MainBorder.BorderBrush as SolidColorBrush;
                if (currentBrush != null)
                {
                    var currentColor = currentBrush.Color;
                    colorDialog.Color = System.Drawing.Color.FromArgb(
                        currentColor.A, currentColor.R, currentColor.G, currentColor.B);
                }
                
                // カスタム色を有効にする
                colorDialog.AllowFullOpen = true;
                colorDialog.FullOpen = true;
                
                // ダイアログを表示
                if (colorDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    var selectedColor = colorDialog.Color;
                    var wpfColor = Color.FromArgb(
                        selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B);
                    ChangeFrameColor(wpfColor);
                }
            }
        }
        
        private void ChangeFrameColor(Color color)
        {
            MainBorder.BorderBrush = new SolidColorBrush(color);
            OnSettingsChanged();
        }
        
        private void ChangeFrameThickness(double thickness)
        {
            MainBorder.BorderThickness = new Thickness(thickness);
            OnSettingsChanged();
        }
        
        private void ResizeToPreset(double width, double height)
        {
            Width = width;
            Height = height;
            OnSettingsChanged();
        }
        
        private void UpdateEdgeHandles()
        {
            if (ResizeHandles == null) return;
            
            // 辺ハンドルを中央に配置
            Canvas.SetLeft(TopHandle, ActualWidth / 2 - 5);
            Canvas.SetLeft(BottomHandle, ActualWidth / 2 - 5);
            Canvas.SetTop(LeftHandle, ActualHeight / 2 - 5);
            Canvas.SetTop(RightHandle, ActualHeight / 2 - 5);
            
            // 角ハンドルの位置を更新
            Canvas.SetRight(TopRightHandle, -5);
            Canvas.SetRight(BottomRightHandle, -5);
            Canvas.SetBottom(BottomLeftHandle, -5);
            Canvas.SetBottom(BottomRightHandle, -5);
        }

        private void SetupResizeHandle(Rectangle handle, string mode)
        {
            handle.MouseLeftButtonDown += (s, e) =>
            {
                if (!_isClickThrough)
                {
                    _isResizing = true;
                    _resizeMode = mode;
                    _resizeStartPoint = e.GetPosition(null);
                    _originalBounds = new Rect(Left, Top, Width, Height);
                    handle.CaptureMouse();
                    e.Handled = true;
                }
            };
            
            handle.MouseMove += (s, e) =>
            {
                if (_isResizing && handle.IsMouseCaptured)
                {
                    var currentPoint = e.GetPosition(null);
                    var deltaX = currentPoint.X - _resizeStartPoint.X;
                    var deltaY = currentPoint.Y - _resizeStartPoint.Y;
                    
                    ResizeWindow(deltaX, deltaY);
                    e.Handled = true;
                }
            };
            
            handle.MouseLeftButtonUp += (s, e) =>
            {
                if (_isResizing)
                {
                    _isResizing = false;
                    _resizeMode = "";
                    handle.ReleaseMouseCapture();
                    // リサイズ終了時に設定を保存
                    OnSettingsChanged();
                    e.Handled = true;
                }
            };
        }
        
        private void ResizeWindow(double deltaX, double deltaY)
        {
            var minWidth = 50;
            var minHeight = 50;
            
            switch (_resizeMode)
            {
                case "TopLeft":
                    Width = Math.Max(minWidth, _originalBounds.Width - deltaX);
                    Height = Math.Max(minHeight, _originalBounds.Height - deltaY);
                    Left = _originalBounds.Left + (_originalBounds.Width - Width);
                    Top = _originalBounds.Top + (_originalBounds.Height - Height);
                    break;
                    
                case "TopRight":
                    Width = Math.Max(minWidth, _originalBounds.Width + deltaX);
                    Height = Math.Max(minHeight, _originalBounds.Height - deltaY);
                    Top = _originalBounds.Top + (_originalBounds.Height - Height);
                    break;
                    
                case "BottomLeft":
                    Width = Math.Max(minWidth, _originalBounds.Width - deltaX);
                    Height = Math.Max(minHeight, _originalBounds.Height + deltaY);
                    Left = _originalBounds.Left + (_originalBounds.Width - Width);
                    break;
                    
                case "BottomRight":
                    Width = Math.Max(minWidth, _originalBounds.Width + deltaX);
                    Height = Math.Max(minHeight, _originalBounds.Height + deltaY);
                    break;
                    
                case "Top":
                    Height = Math.Max(minHeight, _originalBounds.Height - deltaY);
                    Top = _originalBounds.Top + (_originalBounds.Height - Height);
                    break;
                    
                case "Bottom":
                    Height = Math.Max(minHeight, _originalBounds.Height + deltaY);
                    break;
                    
                case "Left":
                    Width = Math.Max(minWidth, _originalBounds.Width - deltaX);
                    Left = _originalBounds.Left + (_originalBounds.Width - Width);
                    break;
                    
                case "Right":
                    Width = Math.Max(minWidth, _originalBounds.Width + deltaX);
                    break;
            }
        }
        
        private void OnMainBorderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isClickThrough && e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                
                // ウィンドウ相対座標で開始点を取得（より安定）
                var startPoint = e.GetPosition(this);
                var screenPoint = PointToScreen(startPoint);
                _dragStartScreenPoint = new POINT { X = (int)screenPoint.X, Y = (int)screenPoint.Y };
                _windowStartPoint = new Point(Left, Top);
                
                // 標準のマウスキャプチャを使用
                MainBorder.CaptureMouse();
                
                e.Handled = true;
            }
        }
        
        private void OnMainBorderMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && MainBorder.IsMouseCaptured)
            {
                // 現在のマウス位置をスクリーン座標で取得
                GetCursorPos(out POINT currentScreenPoint);
                
                // スクリーン座標での移動量を計算
                var deltaX = currentScreenPoint.X - _dragStartScreenPoint.X;
                var deltaY = currentScreenPoint.Y - _dragStartScreenPoint.Y;
                
                // DPIスケーリングを考慮してWPF座標に変換
                var dpiScale = GetDpiScale();
                var scaledDeltaX = deltaX / dpiScale.DpiScaleX;
                var scaledDeltaY = deltaY / dpiScale.DpiScaleY;
                
                Left = _windowStartPoint.X + scaledDeltaX;
                Top = _windowStartPoint.Y + scaledDeltaY;
                
                e.Handled = true;
            }
        }
        
        private void OnMainBorderMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                MainBorder.ReleaseMouseCapture();
                // ドラッグ終了時に設定を保存
                OnSettingsChanged();
                e.Handled = true;
            }
        }
        
        private (double DpiScaleX, double DpiScaleY) GetDpiScale()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                var matrix = source.CompositionTarget.TransformToDevice;
                return (matrix.M11, matrix.M22);
            }
            return (1.0, 1.0);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            EnsureWindowVisible();

            // ウィンドウをクリックスルーにする
            MakeClickThrough();
        }

        private void MakeClickThrough()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            _isClickThrough = true;
            
            // UI要素を非表示
            ResizeHandles.Visibility = Visibility.Collapsed;
            SettingsModeText.Visibility = Visibility.Collapsed;
            MainBorder.Cursor = Cursors.Arrow;
        }

        private void MakeInteractive()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
            _isClickThrough = false;
            
            // UI要素を表示
            ResizeHandles.Visibility = Visibility.Visible;
            SettingsModeText.Visibility = Visibility.Visible;
            MainBorder.Cursor = Cursors.SizeAll;
            
            // ハンドル位置を更新
            UpdateEdgeHandles();
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // ESC で設定モードを終了
            if (e.Key == Key.Escape && !_isClickThrough)
            {
                MakeClickThrough();
            }
        }
        
        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            // マウスがウィンドウ外に出た時の安全機能
            // ただしドラッグ中は通常の動作なので、過度な介入は避ける
        }
        
        protected override void OnDeactivated(EventArgs e)
        {
            // ウィンドウが非アクティブになった時にドラッグを強制終了
            if (_isDragging)
            {
                _isDragging = false;
                MainBorder.ReleaseMouseCapture();
            }
            base.OnDeactivated(e);
        }
        
        private void LoadSettings()
        {
            _settings = SettingsManager.LoadSettings();
            ApplySettings();
        }
        
        private void ApplySettings()
        {
            var defaults = new AppSettings();

            Left = CoerceCoordinate(_settings.Left, defaults.Left);
            Top = CoerceCoordinate(_settings.Top, defaults.Top);
            Width = Math.Max(MinimumWindowSize, CoerceDimension(_settings.Width, defaults.Width));
            Height = Math.Max(MinimumWindowSize, CoerceDimension(_settings.Height, defaults.Height));

            MainBorder.BorderBrush = new SolidColorBrush(_settings.BorderColorValue);
            var thickness = CoerceDimension(_settings.BorderThickness, DefaultBorderThickness);
            MainBorder.BorderThickness = new Thickness(Math.Max(1.0, thickness));

            EnsureWindowVisible();
        }
        
        private static double CoerceCoordinate(double value, double fallback)
        {
            return double.IsNaN(value) || double.IsInfinity(value) ? fallback : value;
        }

        private static double CoerceDimension(double value, double fallback)
        {
            return double.IsNaN(value) || double.IsInfinity(value) || value <= 0 ? fallback : value;
        }

        private void EnsureWindowVisible()
        {
            Width = Math.Max(MinimumWindowSize, Width);
            Height = Math.Max(MinimumWindowSize, Height);

            var virtualLeft = SystemParameters.VirtualScreenLeft;
            var virtualTop = SystemParameters.VirtualScreenTop;
            var virtualRight = virtualLeft + SystemParameters.VirtualScreenWidth;
            var virtualBottom = virtualTop + SystemParameters.VirtualScreenHeight;

            var windowRight = Left + Width;
            var windowBottom = Top + Height;

            var invalidPosition =
                double.IsNaN(Left) || double.IsNaN(Top) ||
                double.IsInfinity(Left) || double.IsInfinity(Top) ||
                windowRight <= virtualLeft || Left >= virtualRight ||
                windowBottom <= virtualTop || Top >= virtualBottom;

            if (invalidPosition)
            {
                var workArea = SystemParameters.WorkArea;

                Width = Math.Min(Width, Math.Max(MinimumWindowSize, workArea.Width));
                Height = Math.Min(Height, Math.Max(MinimumWindowSize, workArea.Height));

                var targetLeft = workArea.Left + (workArea.Width - Width) / 2;
                var targetTop = workArea.Top + (workArea.Height - Height) / 2;

                Left = Clamp(targetLeft, workArea.Left, workArea.Right - Width);
                Top = Clamp(targetTop, workArea.Top, workArea.Bottom - Height);
            }
        }

        private static double Clamp(double value, double min, double max)
        {
            if (min > max)
            {
                return min;
            }

            return Math.Max(min, Math.Min(max, value));
        }

        private void SaveSettings()
        {
            if (_settings != null)
            {
                _settings.Left = Left;
                _settings.Top = Top;
                _settings.Width = Width;
                _settings.Height = Height;
                _settings.BorderColorValue = ((SolidColorBrush)MainBorder.BorderBrush).Color;
                _settings.BorderThickness = MainBorder.BorderThickness.Left;
                
                SettingsManager.SaveSettings(_settings);
            }
        }
        
        private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        
        // 設定が変更された時に自動保存するためのヘルパーメソッド
        private void OnSettingsChanged()
        {
            SaveSettings();
        }
        
        public void ToggleSettingsMode()
        {
            if (_isClickThrough)
            {
                MakeInteractive();
            }
            else
            {
                MakeClickThrough();
            }
        }
    }
}