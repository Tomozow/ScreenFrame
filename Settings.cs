using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace ScreenFrame
{
    public class AppSettings
    {
        public double Left { get; set; } = 100;
        public double Top { get; set; } = 100;
        public double Width { get; set; } = 800;
        public double Height { get; set; } = 600;
        public string BorderColor { get; set; } = "Red";
        public double BorderThickness { get; set; } = 3;
        
        [JsonIgnore]
        public Color BorderColorValue
        {
            get
            {
                try
                {
                    return (Color)ColorConverter.ConvertFromString(BorderColor);
                }
                catch
                {
                    return Colors.Red;
                }
            }
            set
            {
                BorderColor = value.ToString();
            }
        }
    }

    public static class SettingsManager
    {
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ScreenFrame");
        
        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

        public static AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return new AppSettings();
                }

                var json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
            catch (Exception)
            {
                // 設定ファイルの読み込みに失敗した場合はデフォルト設定を返す
                return new AppSettings();
            }
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                // ディレクトリが存在しない場合は作成
                Directory.CreateDirectory(SettingsDirectory);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception)
            {
                // 設定の保存に失敗してもアプリケーションは続行
            }
        }
    }
}