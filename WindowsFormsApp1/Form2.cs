using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using KeyHook;
namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        // 原有成员变量保持不变...

        // 新增键盘钩子相关成员
        private IntPtr _keyboardHookHandle = IntPtr.Zero;
        private const int WH_KEYBOARD_LL = 13;
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private static LowLevelKeyboardProc _keyboardProc;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private static int currentImageIndex;
        private static LockScreenConfig config = ConfigManager.LoadConfig();
        private Timer timer;
        private Timer timer2;
        private Image nextImage;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);
        private TextBox _passwordBox;
        private Label _hintLabel;
        private static readonly Random _random = new Random();
        public Form2()
        {
            InitializeComponent();
            DoubleBuffered = true;
            change_bz();
            //InstallKeyboardHook(); // 新增钩子安装
        }

        // 安装键盘钩子
        private void InstallKeyboardHook()
        {
            _keyboardProc = KeyboardHookCallback;

            using (var currentProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var currentModule = currentProcess.MainModule)
            {
                _keyboardHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardProc,
                    GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        // 键盘钩子回调函数
        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // 拦截Alt+F4
                if (vkCode == (int)Keys.F4 && (IsAltKeyPressed()))
                {
                    return (IntPtr)1; // 阻止系统处理
                }
                // 拦截Alt+tab
                //if (vkCode == (int)Keys.Tab && (IsAltKeyPressed()))
                //{
                //    return (IntPtr)1; // 阻止系统处理
                //}

                //// 拦截Win键（左右Win键分开检测）
                //if (vkCode == (int)Keys.LWin || vkCode == (int)Keys.RWin)
                //{
                //    return (IntPtr)1;
                //}
            }
            return CallNextHookEx(_keyboardHookHandle, nCode, wParam, lParam);
        }

        // 检测Alt键状态
        private bool IsAltKeyPressed()
        {
            return (GetAsyncKeyState(Keys.Menu) & 0x8000) != 0;
        }

        // 修改原有FormClosed事件处理

        public void change_bz()
        {
            currentImageIndex = _random.Next(1, 19);
            string imagePath = @"resources/"+currentImageIndex+".png";
            timer = new Timer();
            timer.Interval = config.WallpaperChangeIntervalInSeconds * 1000;
            timer.Tick += (s, e) =>
            {
                UpdateWallpaper();
            };
            timer.Start();

            // 初始化窗口背景
            this.BackgroundImage = LoadImage(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.FormBorderStyle = FormBorderStyle.None;

            // 窗口最大化
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
        }

        private Image LoadImage(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    return Image.FromStream(ms);
                }
            }
        }

        private void UpdateWallpaper()
        {
            try
            {
                // 预加载下一张图片
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/" + $"{currentImageIndex}.png");
                nextImage = LoadImage(imagePath);

                // 更新背景图片
                if (this.BackgroundImage != null)
                {
                    var oldImage = this.BackgroundImage;
                    this.BackgroundImage = nextImage; // 设置为新图片
                    oldImage.Dispose(); // 释放旧图片资源
                }

                currentImageIndex++;
                if (currentImageIndex > 18)
                {
                    currentImageIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新壁纸时发生错误: {ex.Message}");
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Showpasswordbox();
        }

        public void Showpasswordbox()
        {
            this.TopMost = true;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.Black;
           
            // 密码输入框
            _passwordBox = new TextBox
            {
                PasswordChar = '*',
                Width = 300,
                Height = 40,
                Font = new Font("Arial", 16),
                Location = new Point(
                    (this.Width - 300) / 2,
                    (this.Height - 40) / 2
                )
            };
            _hintLabel = new Label
            {
                Text = "点击输入密码解锁设备",
                Font = new Font("微软雅黑", 20),
                ForeColor = Color.Red,
                //BackColor = Color.Transparent,
                Location = new Point((this.Width - 280) / 2, _passwordBox.Top - 40),
                AutoSize = true
            };
            // 关键：强制获取焦点并选择文本

            //using (Passwordbox passwordForm = new Passwordbox())
            //{
            //    passwordForm.TopMost = true;
            //    if (passwordForm.ShowDialog() == DialogResult.OK)
            //    {
            //        this.Close();
            //    }
            //}
            this.KeyPreview = true;
            this.Controls.Add(_hintLabel);
            this.Controls.Add(_passwordBox);
            this.Shown += (s, e) =>
            {
                _passwordBox.Focus();
                _passwordBox.SelectAll();
            };
            _passwordBox.TextChanged += (s, e) =>
            {
                    if (_passwordBox != null && config.Password == _passwordBox.Text)
                    {
                        this.Close();
                    }
            };
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 停止计时器并释放资源
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (this.BackgroundImage != null)
            {
                this.BackgroundImage.Dispose();
                this.BackgroundImage = null;
            }

            if (nextImage != null)
            {
                nextImage.Dispose();
                nextImage = null;
            }
            if (_keyboardHookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookHandle);
                _keyboardHookHandle = IntPtr.Zero;
            }
            Console.WriteLine("所有资源已释放");
        }
    }
}
