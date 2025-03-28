using Microsoft.Win32;
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
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
        string exePath = $"\"{Application.ExecutablePath}\"";
        public void chkAutoStart_CheckedChanged(bool Isswitch)
        {
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            //获取自启动文件夹的路径
            string startupFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = currentDirectory + "\\ComputerLock_Jhyt.exe.lnk";
            string destinationPath = Path.Combine(startupFolderPath, "ComputerLock_Jhyt.exe.lnk");
            try
            { 
                {
                    if (Isswitch)
                    {
                            ///动态获取程序的相对路径
                        Console.WriteLine("自启成功");

                        File.Copy(shortcutPath, destinationPath, true);
                    }
                    else
                    {
                        File.Delete(destinationPath);
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
