using Microsoft.Win32;

namespace ComputerLock.Hooks;
public class AutostartHook 
{
    private const string RegKey = @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run";
    private const string RegKeySystem = @"Software\Microsoft\Windows\CurrentVersion\Policies\System";
    public bool IsAutostart()
    {
        var registryKey = Registry.LocalMachine.OpenSubKey(RegKey);
        if (registryKey == null)
        {
            return false;
        }
        if (registryKey.GetValue(AppBase.FriendlyName) == null)
        {
            return false;
        }
        return true;
    }

    public void EnabledAutostart()
    {
        string execPath = AppBase.ExecutablePath;
        var registryKey = Registry.LocalMachine.CreateSubKey(RegKey);
        registryKey.SetValue(AppBase.FriendlyName, $"\"{execPath}\"");
        registryKey.Close();
    }

    public void DisabledAutostart()
    {
        var registryKey = Registry.LocalMachine.CreateSubKey(RegKey);
        registryKey.DeleteValue(AppBase.FriendlyName);
        registryKey.Close();
    }
    public void DisableWindowsLockScreen()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegKeySystem))
            {
                if (key != null)
                {
                    key.SetValue("DisableLockWorkstation", 1, RegistryValueKind.DWord);
                    Console.WriteLine("Windows 锁屏功能已禁用。");
                }
                else
                {
                    Console.WriteLine("无法创建或打开注册表项。");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"操作失败：{ex.Message}");
        }
    }
    public void enableWindowsLockScreen()
    {
        try
        {
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegKeySystem))
            {
                if (key != null)
                {
                    key.SetValue("DisableLockWorkstation", 0, RegistryValueKind.DWord);
                    Console.WriteLine("Windows 锁屏功能已禁用。");
                }
                else
                {
                    Console.WriteLine("无法创建或打开注册表项。");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"操作失败：{ex.Message}");
        }
    }
}