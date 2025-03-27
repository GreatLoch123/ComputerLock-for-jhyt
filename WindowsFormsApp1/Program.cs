using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using FormsTimer = System.Windows.Forms.Timer;
using System.Threading;
using KeyHook;
using System.IO;
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
        private static readonly UserActivityMonitor _monitor = new UserActivityMonitor();
        private static HotKeyManager _hotKeyManager;
        private static LockKeyboardHook _lockHook;
        private static regedit_edit Regedit_Edit = new regedit_edit();
        [STAThread]
        static void Main(string[] args)
        {
            bool isOnlyInstance;
            bool isRestart = args.Contains("/restart");
            if (config.Isfirst)
            {
                Regedit_Edit.chkAutoStart_CheckedChanged(true);
                config.Isfirst = false;
                ConfigManager.SaveConfig(config);
            }
            using (Mutex mutex = new Mutex(true, "WindowsFormsApp1_LockScreen", out isOnlyInstance))
            {

                // 如果不是唯一实例，直接退出程序
                if (!isOnlyInstance && !isRestart)
                {
                    MessageBox.Show("程序已在运行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                _lockHook = new LockKeyboardHook();
              
            _monitor.Initialize(config.LockTimeInSeconds); // 10分钟无操作锁定
                _monitor.OnIdle += Monitor_OnIdle;
                _monitor.Start();
                //ShowSingleInstanceForm<Form1>();

                //ConfigManager.SaveConfig(new LockScreenConfig
                //{
                //    LockTimeInSeconds = 600,
                //    Password = "1"
                //});
                lockScreenForm = new Form2();
                using ( trayIcon = new NotifyIcon())
                {
                    trayIcon.Icon = new Icon("Resources/icon.ico"); // 替换为自定义图标
                    trayIcon.Text = "服务已启动，点击显示主界面";
                    trayIcon.Visible = true;

                    // 初始化托盘菜单
                    ContextMenu trayMenu = new ContextMenu();
                    trayMenu.MenuItems.Add("锁屏", (sender, e) =>
                    {
                        ////KjjHook.UninstallHook();
                        //lockScreenForm = new Form2();
                        //lockScreenForm.Show();
                        LockSystem();
                        //KjjHook.InstallHook();
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
      
                                // 如果未打开，则创建新实例并显示
                                SettingForm = new Form1();
                                SettingForm.Show();
                        }
                    };
                    trayIcon.ContextMenu = trayMenu;
                    //using (var hiddenForm = new Form())
                    //{
                    //    hiddenForm.ShowInTaskbar = false;
                    //    hiddenForm.Size = Size.Empty;
                    //    hiddenForm.Opacity = 0;
                    //    hiddenForm.ControlBox = false;
                    //    // 初始化热键管理器
                    //    _hotKeyManager = new HotKeyManager(hiddenForm.Handle);
                    //    _hotKeyManager.HotKeyPressed += () =>
                    //    {
                    //        LockSystem();
                    //    };

                    //    // 初始化锁屏钩子
                    //    _lockHook = new LockKeyboardHook();

                    //    Application.Run(hiddenForm);
                    //}

                    // 运行消息循环
                    Application.Run();
                }
            }
        }
        private static void LockSystem()
        {
            
            _lockHook.SetLockState(true);
            // 显示锁屏界面
            var lockForm = new Form2();
            lockForm.FormClosed += (s, e) =>
            {
                _lockHook.SetLockState(false);
            };
            lockForm.ShowDialog();
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
            ////KjjHook.UninstallHook();
            //KjjHook.SetLockScreenState(true);
            var uiThread = new Thread(() =>
            {
                trayIcon.Visible = false;
                var lockForm = new Form2();
                Application.Run(lockForm); // 使用Application.Run保证消息循环
                //KjjHook.InstallHook();
                //KjjHook.SetLockScreenState(false);
                trayIcon.Visible = true;
            });

            uiThread.SetApartmentState(ApartmentState.STA);
            uiThread.Start();
        }
        // 显示锁屏窗体
        private static void ShowLockScreen()
        {
            if (lockScreenForm != null && !lockScreenForm.IsDisposed)
            {
                if (lockScreenForm.InvokeRequired)
                {
                    lockScreenForm.Invoke(new Action(() => lockScreenForm.Show()));
                }
                else
                {
                    lockScreenForm.Show();
                }
                return;
            }

            // 创建新窗体时确保在UI线程
            Form mainForm = Application.OpenForms.Cast<Form>().FirstOrDefault();
            if (mainForm != null && mainForm.InvokeRequired)
            {
                mainForm.BeginInvoke(new Action(() =>
                {
                    ////KjjHook.UninstallHook();
                    SettingForm?.Close();
                    lockScreenForm = new Form2();
                    lockScreenForm.FormClosed += (s, e) =>
                    {
                        //KjjHook.InstallHook();
                    };
                    lockScreenForm.Show();
                }));
            }
            else
            {
                ////KjjHook.UninstallHook();
                SettingForm?.Close();
                lockScreenForm = new Form2();
                lockScreenForm.FormClosed += (s, e) =>
                {
                    //KjjHook.InstallHook();
                };
                lockScreenForm.Show();
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
        private static void Monitor_OnIdle(object sender, EventArgs e)
        {
            // 获取主窗体（隐藏的）
            Console.WriteLine("触发成功");
            _monitor.Stop();
            LockSystem();
            _monitor.Start();
        }



    }
}