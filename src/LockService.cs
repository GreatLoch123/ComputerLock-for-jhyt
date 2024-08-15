using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Threading;

namespace ComputerLock;
public class LockService
{
    private bool _isLocked = false;
    private readonly IServiceProvider _serviceProvider;
    private WindowLockScreen? _windowLockScreen;
    private readonly List<WindowBlankScreen> _blankScreens = [];
    private readonly IStringLocalizer<Lang> _lang;
    private readonly AppSettings _appSettings;
    public event EventHandler? OnLock;
    public event EventHandler? OnUnlock;
    public LockService(IServiceProvider serviceProvider, IStringLocalizer<Lang> lang, AppSettings appSettings)
    {
        _serviceProvider = serviceProvider;
        _lang = lang;
        _appSettings = appSettings;

        // 防止锁屏时系统崩溃、重启等问题导致任务栏被禁用
        // 启动时默认启用一次
    }

    public void Lock()
    {
        if (_isLocked)
        {
            return;
        }
        var primaryScreen = Screen.PrimaryScreen;
        if (primaryScreen == null)
        {
            throw new Exception("没有检测到屏幕 no screen");
        }

        _isLocked = true;
        if (_appSettings.LockAnimation)
        {
            ShowPopup(_lang["Locked"]);
        }

        //_taskManagerHook.DisabledTaskManager();
        if (_blankScreens.Count > 0)
        {
            _blankScreens.Clear();
        }

        _windowLockScreen = _serviceProvider.GetRequiredService<WindowLockScreen>();
        _windowLockScreen.Left = primaryScreen.WorkingArea.Left;
        _windowLockScreen.Top = primaryScreen.WorkingArea.Top;
        _windowLockScreen.OnUnlock += FmLockScreen_OnUnlock;
        _windowLockScreen.Closing += (_, _) =>
        {
            _windowLockScreen.OnUnlock -= FmLockScreen_OnUnlock;
        };
        _windowLockScreen.Show();
        _windowLockScreen.Activate();
        for (var i = 0; i <= Screen.AllScreens.Length - 1; i++)
        {
            var screen = Screen.AllScreens[i];
            if (screen.Primary)
            {
                continue;
            }

            var blankScreen = _serviceProvider.GetRequiredService<WindowBlankScreen>();
            blankScreen.WindowStartupLocation = WindowStartupLocation.Manual;
            blankScreen.Left = screen.WorkingArea.Left;
            blankScreen.Top = screen.WorkingArea.Top;
            blankScreen.OnDeviceInput += BlankScreen_OnDeviceInput;
            blankScreen.Show();
            blankScreen.Activate();
            _blankScreens.Add(blankScreen);
        }

        OnLock?.Invoke(this, EventArgs.Empty);
    }

    private void FmLockScreen_OnUnlock(object? sender, EventArgs e)
    {
        foreach (var screen in _blankScreens)
        {
            screen.OnDeviceInput -= BlankScreen_OnDeviceInput;
            screen.Unlock();
            screen.Close();
        }
        _isLocked = false;
        if (_appSettings.LockAnimation)
        {
            ShowPopup(_lang["UnLocked"]);
        }
        OnUnlock?.Invoke(this, EventArgs.Empty);
    }
    private void BlankScreen_OnDeviceInput(object? sender, EventArgs e)
    {
        _windowLockScreen?.ShowPassword();
    }

    private void ShowPopup(string message)
    {
        var popup = new WindowPopup(message);
        double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
        double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
        popup.Left = (primaryScreenWidth - popup.Width) / 2;
        popup.Top = (primaryScreenHeight - popup.Height) / 2;
        popup.Show();

        var timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(1100),
        };
        timer.Tick += (_, __) =>
        {
            timer.Stop();
            popup.CloseWindow();
        };
        timer.Start();
    }
}