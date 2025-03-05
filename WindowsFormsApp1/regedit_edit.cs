using Microsoft.Win32;
using System;
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
        public void chkBlockWinL_CheckedChanged()
        {
            try
            {
                using (RegistryKey policyKey = Registry.CurrentUser.CreateSubKey(POLICIES_REGISTRY_KEY))
                {
                    if (config.UseSystemLock)
                    {
                        policyKey.SetValue("DisableLockWorkstation", 1, RegistryValueKind.DWord);
                    }
                    else
                    {
                        policyKey.DeleteValue("DisableLockWorkstation", false);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改默认锁屏启动项失败：{ex.Message}");
            }
        }
    }
}
