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

        private static LockScreenConfig config = ConfigManager.LoadConfig();

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

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                StartMonitor();
                SettingForm = new Form1();
                SettingForm.Show();
                //ConfigManager.SaveConfig(new LockScreenConfig
                //{
                //    LockTimeInSeconds = 600,
                //    Password = "1"
                //});
                using (NotifyIcon trayIcon = new NotifyIcon())
                {
                    trayIcon.Icon = new Icon("Resources/icon.ico"); // 替换为自定义图标
                    trayIcon.Text = "服务已启动，点击显示主界面";
                    trayIcon.Visible = true;

                    // 初始化托盘菜单
                    ContextMenu trayMenu = new ContextMenu();
                    trayMenu.MenuItems.Add("锁屏", (sender, e) =>
                    {
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
        private static void ShowSingleInstanceForm<T>() where T : Form, new()
        {
            // 检查当前是否已有指定类型的窗体打开
            Form existingForm = Application.OpenForms.Cast<Form>().FirstOrDefault(f => f is T);
            if (existingForm != null)
            {
                // 如果已打开，则激活窗口
                existingForm.WindowState = FormWindowState.Normal; // 恢复窗口
                existingForm.Activate(); // 激活窗口
            }
            else
            {
                // 如果未打开，则创建新实例并显示
                T form = new T();
                form.Show();
            }
        }
        // 显示锁屏窗体
        private static void ShowLockScreen()
        {
            try
            {
                // 如果锁屏窗体不存在或已被释放，则创建新的实例
                if (lockScreenForm == null || lockScreenForm.IsDisposed)
                {
                    lockScreenForm = new Form2();
                    lockScreenForm.FormClosing += (s, e) =>
                    {
                        lockScreenForm = null; // 清空引用
                    };
                    lockScreenForm.FormClosed += (s, e) =>
                    {
                        StartMonitor();
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