using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class regedit_edit
    {
        // 新增注册表路径定义
        private const string RUN_REGISTRY_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string POLICIES_REGISTRY_KEY = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
        string batFilePath = "DisableSysLock.bat";
        private  string APP_NAME = Assembly.GetEntryAssembly().GetName().Name;
        private static int currentImageIndex = 2;
        private static LockScreenConfig config = ConfigManager.LoadConfig();
        // 开机自启控制
        public void chkAutoStart_CheckedChanged(bool Isswitch)
        {
            try
            {
                string exePath = $"\"{Application.ExecutablePath}\"";
                using (RegistryKey runKey = Registry.CurrentUser.CreateSubKey(RUN_REGISTRY_KEY))
                {
                    if (Isswitch)
                    {
                        runKey.SetValue(APP_NAME, exePath);
                        Console.WriteLine("添加自启成功");
                        Console.WriteLine(APP_NAME);

                    }
                    else
                    {
                        runKey.DeleteValue(APP_NAME, false);
                        Console.WriteLine("删除自启成功");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改启动项失败：{ex.Message}");
            }
        }

        // Win+L屏蔽控制
        public void chkBlockWinL_CheckedChanged(bool Isswitch)
        {
            try
            {
                    ProcessStartInfo processInfo = new ProcessStartInfo();

                    // 设置批处理文件路径
                    processInfo.FileName = batFilePath;
                    processInfo.UseShellExecute = true; // 使用 Shell 执行

                    // 启动进程
                    Process process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    // 等待进程结束（可选）
                    process.WaitForExit();

                    // 获取退出代码（可选）
                    int exitCode = process.ExitCode;
                    Console.WriteLine("批处理文件执行完成，退出代码: " + exitCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改默认锁屏启动项失败：{ex.Message}");
            }
        }
    }
}
