﻿using System.Diagnostics;
using System.IO;
using System.Windows;
using ComputerLock.Hooks;
using Microsoft.Win32;

namespace ComputerLock;
public partial class WindowMain : Window, IDisposable
{
    private readonly SystemKeyHook _systemKeyHook;
    private readonly AutostartHook _autostartHook;
    private readonly MemoryCleaner _memoryCleaner;
    private readonly AppSettings _appSettings;
    private readonly AppSettingWriter _appSettingWriter;
    private readonly UserActivityMonitor? _activityMonitor;
    private readonly ILocker _locker;
    private readonly IWindowTitleBar _IWindowTitleBar;
    private readonly NotifyIcon _notifyIcon = new();
    private readonly ContextMenuStrip _contextMenuStrip = new();


    public WindowMain(AutostartHook autostartHook, AppSettings appSettings, ILocker locker, UserActivityMonitor activityMonitor,SystemKeyHook systemKeyHook, IWindowTitleBar iWindowTitleBar, AppSettingWriter appSettingWriter)
    {
        InitializeComponent();
        _systemKeyHook = systemKeyHook;
        _appSettings = appSettings;
        _locker = locker;
        _systemKeyHook.DisableSystemKey();
        _IWindowTitleBar = iWindowTitleBar;
        _autostartHook = autostartHook;
        _appSettingWriter = appSettingWriter;
        var app = System.Windows.Application.Current;
        InitializeNotifyIcon();
        if (_appSettings.Fisrtload == 1)
        {
            _autostartHook.EnabledAutostart();
            _autostartHook.DisableWindowsLockScreen();
            _appSettings.Fisrtload = 0;
            _appSettingWriter.Save(_appSettings);
            ((App)app).CloseMainWindow();
        }
       
        if (_appSettings.AutoLockSecond != 0)
        {
            _activityMonitor = activityMonitor;
            _activityMonitor.Init(_appSettings.AutoLockSecond);
            _activityMonitor.OnIdle += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    _locker.Lock();
                });
            };
            _locker.OnLock += (_, _) =>
            {
                _activityMonitor.StopMonitoring();
            };
            _locker.OnUnlock += (_, _) =>
            {
                _activityMonitor.StartMonitoring();
            };
            _activityMonitor.StartMonitoring();

            SystemEvents.SessionSwitch += (_, e) =>
            {
                if (e.Reason == SessionSwitchReason.SessionLock)
                {
                    _activityMonitor.StopMonitoring();
                }
                else if (e.Reason == SessionSwitchReason.SessionUnlock)
                {
                    _activityMonitor.StartMonitoring();
                }
            };
        }

        if (_appSettings.LockOnStartup)
        {
            _locker.Lock();
        }
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        this.Title = Lang.Title;
        this.WindowState = WindowState.Normal;
    }

    private void InitializeNotifyIcon()
    {
        var app = System.Windows.Application.Current;
        var btnShowWindow = new ToolStripMenuItem(Lang.ShowMainWindow);
        btnShowWindow.Click += (_, _) => ((App)app).CloseMainWindow();
        _contextMenuStrip.Items.Add(btnShowWindow);

        var btnLock = new ToolStripMenuItem(Lang.DoLock);
        btnLock.Click += (_, _) =>
        {
            _locker.Lock();
        };
        _contextMenuStrip.Items.Add(btnLock);

        var btnClose = new ToolStripMenuItem(Lang.Exit);
        btnClose.Click += (_, _) =>
        {
            System.Windows.Application.Current.Shutdown();
            MemoryCleaner.ClearMemory();
        };
        _contextMenuStrip.Items.Add(btnClose);

        _notifyIcon.ContextMenuStrip = _contextMenuStrip;
        Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/ComputerLock;component/icon.ico")).Stream;
        _notifyIcon.Icon = new Icon(iconStream);
        _notifyIcon.Text = Lang.Title;
        _notifyIcon.Click += (object? sender, EventArgs e) =>
        {
            var args = e as MouseEventArgs;
            if (args is not { Button: MouseButtons.Left })
            {
                return;
            }
            ((App)app).CloseMainWindow();
            //Form1 myForm = new Form1();
            //myForm.ShowDialog();  // 模态显示窗体，阻塞当前线程，直到窗体关闭

        };
        _notifyIcon.Visible = true;
    }

    private void ShowMainWindow()
    {
        this.ShowInTaskbar = true;
        this.WindowState = WindowState.Normal;
        this.Activate();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (this.WindowState == WindowState.Minimized)
        {
            this.ShowInTaskbar = false;
            _IWindowTitleBar.Restart();
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_appSettings.IsHideWindowWhenClose)
        {
            this.WindowState = WindowState.Minimized;
            e.Cancel = true;

        }
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
        }
    }
    public void Dispose()
    {
        _notifyIcon.Dispose();
        _activityMonitor?.Dispose();
    }
}