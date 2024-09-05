using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace ComputerLock;
/// <summary>
/// WindowLockScreen.xaml 的交互逻辑
/// </summary>
public partial class WindowLockScreen : Window
{
    private DateTime _hideSelfTime;
    
    private readonly int _hideSelfSecond = 10;    
    private readonly DispatcherTimer _timer = new();
    private readonly AppSettings _appSettings;
    private readonly IStringLocalizer<Lang> _lang;
    private DispatcherTimer wallpaperTimer;
    private int currentImageIndex = 1;
    private int totalImages = 18; // 假设你有18张图片
    public event EventHandler<EventArgs>? OnUnlock;

    /// <summary>
    /// 引用user32.dll动态链接库（windows api），
    /// 使用库中定义 API：SetCursorPos 
    /// </summary>
    [DllImport("user32.dll")]
    private static extern int SetCursorPos(int x, int y);
    /// <summary>
    /// 移动鼠标到指定的坐标点
    /// </summary>
    public void MoveMouseToPoint(Point p)
    {
        SetCursorPos((int)p.X, (int)p.Y);
    }

    //点击事件
    [DllImport("User32")]
    private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);
    public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
    public const int MOUSEEVENTF_LEFTUP = 0x0004;
    public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
    public const int MOUSEEVENTF_RIGHTUP = 0x0010;
    private readonly IWindowTitleBar _IWindowTitleBar;
    public WindowLockScreen(AppSettings appSettings, IStringLocalizer<Lang> lang, IWindowTitleBar iWindowTitleBar)
    {
        InitializeComponent();
        _appSettings = appSettings;
        _lang = lang;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();
        InitializeWallpaperTimer();
        Random random = new Random();
        int randomNumber = random.Next(1, 19); // 生成1到18的随机数（包含1，包含18）
        currentImageIndex = randomNumber;
        UpdateWallpaper();
        _IWindowTitleBar = iWindowTitleBar;
    }

    public void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LblPassword.Content = _lang["Password"];

        if (_appSettings.EnablePasswordBox)
        {
            if (_appSettings.IsHidePasswordWindow)
            {
                LblMessage.Visibility = Visibility.Visible;
                LblMessage.Content = $"{_lang["TimerPrefix"]}{_hideSelfSecond}{_lang["TimerPostfix"]}";
            }
            RefreshHideSelfTime();
        }
        HidePassword();
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        switch (_appSettings.PasswordInputLocation)
        {
            case ScreenLocationEnum.Center:
                PasswordBlock.VerticalAlignment = VerticalAlignment.Center;
                PasswordBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                break;
            case ScreenLocationEnum.TopLeft:
                PasswordBlock.VerticalAlignment = VerticalAlignment.Top;
                PasswordBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                break;
            case ScreenLocationEnum.TopRight:
                PasswordBlock.VerticalAlignment = VerticalAlignment.Top;
                PasswordBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                break;
            case ScreenLocationEnum.BottomLeft:
                PasswordBlock.VerticalAlignment = VerticalAlignment.Bottom;
                PasswordBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                break;
            case ScreenLocationEnum.BottomRight:
                PasswordBlock.VerticalAlignment = VerticalAlignment.Bottom;
                PasswordBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                break;
            default:
                PasswordBlock.VerticalAlignment = VerticalAlignment.Center;
                PasswordBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                break;
        }
    }

    public void ShowPassword()
    {
        if (_appSettings.EnablePasswordBox)
        {
            RefreshHideSelfTime();
            TxtPassword.Visibility = Visibility.Visible;
            PasswordBlock.Opacity = 1;
        }
        else
        {
            TxtPassword.Visibility = Visibility.Visible;
            //TxtPassword.Width = 1;
            PasswordBlock.Width = 1;
            PasswordBlock.Opacity = 0.01;
        }
        //TODO 处理密码      
        TxtPassword.Password = "";
        TxtPassword.Focus();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        try
        {
            var time = DateTime.Now;
            if (time.Second % 50 == 0)
            {
                if (_appSettings.IsDisableWindowsLock)
                {
                    DoMoveMouse();
                }
            }
            if (_appSettings.EnablePasswordBox)
            {
                if (_appSettings.IsHidePasswordWindow)
                {
                    var hideCountdown = Convert.ToInt32(_hideSelfTime.Subtract(time).TotalSeconds);
                    LblMessage.Content = $"{_lang["TimerPrefix"]}{hideCountdown}{_lang["TimerPostfix"]}";
                    if (hideCountdown <= 0)
                    {
                        HidePassword();
                    }
                }
            }
        }
        catch (Exception ex)
        {
        }
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_appSettings.EnablePasswordBox)
        {
            RefreshHideSelfTime();
        }
        var txt = TxtPassword.Password;
        if (txt.IsEmpty())
        {
            return;
        }
        if (_appSettings.Password != JiuLing.CommonLibs.Security.MD5Utils.GetStringValueToLower(txt))
        {
            return;
        }
        OnUnlock?.Invoke(this, EventArgs.Empty);
        this.Close();
    }

    private void TxtPassword_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Enter || e.Key == Key.Escape)
        {
            TxtPassword.Password = "";
        }
        if (Keyboard.IsKeyToggled(Key.CapsLock))
        {
            LblCapsLockWarning.Visibility = Visibility.Visible; // 显示大写锁定提示
        }
        else
        {
            LblCapsLockWarning.Visibility = Visibility.Collapsed; // 隐藏大写锁定提示
        }
    }
    private void DoMoveMouse()
    {
        var random = new Random();
        var x = random.Next(0, 100);
        var y = random.Next(0, 100);

        MoveMouseToPoint(new Point(x, y));
        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, x, y, 0, 0);
    }
    private void HidePassword()
    {
        if (PasswordBlock.Opacity == 1)
        {
            TxtPassword.Visibility = Visibility.Collapsed;
            PasswordBlock.Opacity = 0;
        }
    }
    private void RefreshHideSelfTime()
    {
        _hideSelfTime = DateTime.Now.AddSeconds(_hideSelfSecond);
        this.Dispatcher.BeginInvoke(new Action(() =>
        {
            LblMessage.Content = $"{_lang["TimerPrefix"]}{_hideSelfSecond}{_lang["TimerPostfix"]}";
        }));
    }

    private void PasswordBlock_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_appSettings.EnablePasswordBox)
        {
            if ((_appSettings.PasswordBoxActiveMethod & PasswordBoxActiveMethodEnum.MouseDown) != PasswordBoxActiveMethodEnum.MouseDown)
            {
                return;
            }
            ShowPassword();
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        
    }
    private void Window_Closed(object sender, EventArgs e)
    {
        // 窗口关闭后执行的逻辑
        _timer.Stop();
        _timer.Tick -= Timer_Tick;  // 清除事件处理程序
        wallpaperTimer.Stop();
        wallpaperTimer.Tick -= WallpaperTimer_Tick;  // 清除事件处理程序
        TxtPassword.PasswordChanged -= TxtPassword_PasswordChanged;
        TxtPassword.KeyDown -= TxtPassword_KeyDown;
        PasswordBlock.MouseDown -= PasswordBlock_MouseDown;
        _IWindowTitleBar.Restart();
    }
    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
        {
            return;
        }
        if (_appSettings.EnablePasswordBox)
        {
            if ((_appSettings.PasswordBoxActiveMethod & PasswordBoxActiveMethodEnum.KeyboardDown) != PasswordBoxActiveMethodEnum.KeyboardDown)
            {
                return;
            }
        }
        ShowPassword();
    }
    private void InitializeWallpaperTimer()
    {
        wallpaperTimer = new DispatcherTimer();
        wallpaperTimer.Interval = TimeSpan.FromSeconds(30); // 壁纸计时器每30秒触发一次
        wallpaperTimer.Tick += WallpaperTimer_Tick;
        wallpaperTimer.Start();
    }
    private void WallpaperTimer_Tick(object sender, EventArgs e)
    {
        UpdateWallpaper();
    }
    private void UpdateWallpaper()
    {
       
        if (this.Background is ImageBrush oldBrush)
        {
            oldBrush.ImageSource = null;
            this.Background = null;
        }
        // 构建相对路径的 URI
        string imagePath = $"pack://application:,,,/Resources/{currentImageIndex}.png";
        ImageBrush imageBrush = new ImageBrush
        {
            ImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute))
        };
        this.Background = imageBrush;
        currentImageIndex++;
        if (currentImageIndex > totalImages)
        {
            currentImageIndex = 1;
        }
    }

}
