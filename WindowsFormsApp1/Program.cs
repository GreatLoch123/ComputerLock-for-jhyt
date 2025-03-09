using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FormsTimer = System.Windows.Forms.Timer;
using System.Threading;
using KeyHook;
using Microsoft.Win32; //写入注册表时要用到
using ComputerLock.Hooks;
using System.Linq;
namespace WindowsFormsApp1
{
    internal static class Program
    {
        private static Form2 lockScreenForm = null; // 锁屏窗体
        private static Form1 SettingForm = null; // 设置窗体
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LockScreenConfig config = ConfigManager.LoadConfig();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private static NotifyIcon trayIcon; //声明图标
        private static IntPtr _hookHandle = IntPtr.Zero;
        private static LowLevelKeyboardProc _hookProc;
        private static DateTime _lastTriggerTime = DateTime.MinValue;
        [STAThread]
        static void Main()
        {
            bool isOnlyInstance;
            using (Mutex mutex = new Mutex(true, "WindowsFormsApp1_LockScreen", out isOnlyInstance))
            {
                // 如果不是唯一实例，直接退出程序
                if (!isOnlyInstance)
                {
                    MessageBox.Show("程序已在运行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                KjjHook.InstallHook();
                KjjHook.OnWinLDetected += ShowLockScreen;
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                StartMonitor();
                ShowSingleInstanceForm<Form1>();
                //ConfigManager.SaveConfig(new LockScreenConfig
                //{
                //    LockTimeInSeconds = 600,
                //    Password = "1"
                //});
                using ( trayIcon = new NotifyIcon())
                {
                    trayIcon.Icon = new Icon("Resources/icon.ico"); // 替换为自定义图标
                    trayIcon.Text = "服务已启动，点击显示主界面";
                    trayIcon.Visible = true;

                    // 初始化托盘菜单
                    ContextMenu trayMenu = new ContextMenu();
                    trayMenu.MenuItems.Add("锁屏", (sender, e) =>
                    {
                        KjjHook.UninstallHook();
                        lockScreenForm = new Form2();
                        lockScreenForm.Show();
                    });
                    trayMenu.MenuItems.Add("退出", (sender, e) =>
                    {
                        trayIcon.Visible = false;
                        trayIcon.Dispose();
                        Application.Exit();
                    });
                    trayIcon.MouseClick += (sender, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            ShowSingleInstanceForm<Form1>();
                        }
                    };
                    trayIcon.ContextMenu = trayMenu;

                    // 运行消息循环
                    Application.Run();
                }
            }
        }
        public static void InstallHook()
        {
            _hookProc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        public static void UninstallHook()
        {
            UnhookWindowsHookEx(_hookHandle);
        }
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // 检测 Win + L
                if (vkCode == (int)Keys.L && IsWinKeyPressed())
                {
                    // 防抖：500ms 内只触发一次
                    if ((DateTime.Now - _lastTriggerTime).TotalMilliseconds < 500)
                    {

                        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
                    }
                    _lastTriggerTime = DateTime.Now;

                    // 阻止系统处理 Win+L
                    UninstallHook(); // 先卸载钩子，防止后续冲突

                    // 显示自定义锁屏界面（确保在 UI 线程中操作）
                    Form2 _lockScreenForm = new Form2();
                    _lockScreenForm.FormClosed += (s, e) => InstallHook(); // 界面关闭后重新安装钩子
                    _lockScreenForm.Show();

                    // 返回 1 表示已处理该事件，阻止系统进一步处理
                    return (IntPtr)1;

                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private static bool IsWinKeyPressed()
        {
            return (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0;
        }
        private static void ShowSingleInstanceForm<T>() where T : Form1, new()
        {
            // 检查当前是否已有指定类型的窗体打开
            SettingForm = Application.OpenForms.Cast<Form1>().FirstOrDefault(f => f is T);
            if (SettingForm != null)
            {
                // 如果已打开，则激活窗口
                SettingForm.WindowState = FormWindowState.Normal; // 恢复窗口
                SettingForm.Activate(); // 激活窗口
            }
            else
            {
                // 如果未打开，则创建新实例并显示
                SettingForm = new T();
                SettingForm.Show();
            }
        }
        //测试
        private static void Test()
        {
            //KjjHook.UninstallHook();
            KjjHook.SetLockScreenState(true,true);
            var uiThread = new Thread(() =>
            {
                var lockForm = new Form2();
                Application.Run(lockForm); // 使用Application.Run保证消息循环
                KjjHook.SetLockScreenState(false,false);
            });

            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
        }
        // 显示锁屏窗体
        private static void ShowLockScreen()
        {
            try
            {
                
                // 如果锁屏窗体不存在或已被释放，则创建新的实例
                if (lockScreenForm == null || lockScreenForm.IsDisposed)
                {
                    KjjHook.UninstallHook();
                    SettingForm.Close();
                    lockScreenForm = new Form2();
                    lockScreenForm.FormClosing += (s, e) =>
                    {
                        lockScreenForm = null; // 清空引用
                    };
                    lockScreenForm.FormClosed += (s, e) =>
                    {
                        StartMonitor();
                        KjjHook.InstallHook();
                    };
                }

                if (!lockScreenForm.Visible)
                {
                    lockScreenForm.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"锁屏窗口显示错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 显示主窗体
        private static void ShowForm(Form form)
        {
            try
            {
                if (!form.Visible)
                {
                    form.Show();
                }
                form.WindowState = FormWindowState.Normal;
                form.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"主界面显示错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 重置无操作计时器
        private static void StartMonitor()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            UserActivityMonitor Monitor = new UserActivityMonitor();
            Monitor.Init(config.LockTimeInSeconds);

            // 注册空闲事件
            Monitor.OnIdle += (sender, e) =>
            {
                ShowLockScreen();
                Monitor.Dispose();
            };
            Monitor.StartMonitoring();
        }
    }
}