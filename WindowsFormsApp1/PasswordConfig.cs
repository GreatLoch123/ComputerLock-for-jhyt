using System;
using System.IO;

namespace WindowsFormsApp1
{
    public class LockScreenConfig
    {
        public int LockTimeInSeconds { get; set; } // 锁屏时间（秒）
        public string Password { get; set; } // 锁屏密码
        public bool UseSystemLock { get; set; } // 是否使用系统锁屏
        public bool AutoStart { get; set; }
        public int WallpaperChangeIntervalInSeconds { get; set; } // 壁纸切换时间（秒）
    }

    public static class ConfigManager
    {
        private static readonly string ConfigFilePath = "LockScreenConfig.txt";

        // 保存配置到文件
        public static void SaveConfig(LockScreenConfig config)
        {
            try
            {
                File.WriteAllText(ConfigFilePath,
                    $"{config.LockTimeInSeconds}\n" +
                    $"{config.Password}\n" +
                    $"{config.UseSystemLock}\n" +
                     $"{config.AutoStart}\n" +
                    $"{config.WallpaperChangeIntervalInSeconds}");
                Console.WriteLine("配置已保存");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置时出错: {ex.Message}");
            }
        }

        // 从文件加载配置
        public static LockScreenConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string[] lines = File.ReadAllLines(ConfigFilePath);
                    if (lines.Length >= 5 &&
                        int.TryParse(lines[0], out int lockTimeInSeconds) &&
                        bool.TryParse(lines[2], out bool useSystemLock) &&
                        bool.TryParse(lines[3], out bool autostart) &&
                        int.TryParse(lines[4], out int wallpaperChangeIntervalInSeconds))
                    {
                        return new LockScreenConfig
                        {
                            LockTimeInSeconds = lockTimeInSeconds,
                            Password = lines[1],
                            UseSystemLock = useSystemLock,
                            AutoStart = autostart,
                            WallpaperChangeIntervalInSeconds = wallpaperChangeIntervalInSeconds
                        };
                    }
                }

                Console.WriteLine("配置文件不存在或格式错误，生成默认配置");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置时出错: {ex.Message}");
            }

            // 配置文件不存在或格式错误，写入默认配置
            var defaultConfig = new LockScreenConfig
            {
                LockTimeInSeconds = 180, // 默认锁屏时间
                Password = "1", // 默认密码
                UseSystemLock = false, // 默认不使用系统锁屏
                AutoStart = true,
                WallpaperChangeIntervalInSeconds = 180 // 默认壁纸切换时间
            };

            SaveConfig(defaultConfig); // 写入默认配置
            return defaultConfig;
        }
    }
}