using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using WindowsFormsApp1;
using System.Reflection.Emit;
namespace KeyHook
{
    class KjjHook
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

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
        private static bool _isLockScreenActive = false; //获取锁屏界面状况
        private static bool _isUsewin = false; //获取锁屏界面状况
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_LMENU = 0xA4;
        private const int WM_SYSKEYDOWN = 0x0104;  // 系统按键按下事件
        private const int VK_MENU = 0x12;
        private static IntPtr _hookHandle = IntPtr.Zero;
        private static LowLevelKeyboardProc _hookProc;
        private static DateTime _lastTriggerTime = DateTime.MinValue;
        public static event Action OnWinLDetected;
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
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // 优先处理锁屏状态下的拦截
                if (_isLockScreenActive)
                {
                    // 拦截Win键（左右Win键分开检测）
                   
                    // 拦截Alt+tab（需同时检测Alt键状态）
                    if (wParam == (IntPtr)WM_SYSKEYDOWN &&
                        vkCode == (int)Keys.Tab &&
                        (GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
                    {
                        Console.WriteLine("Alt+Tab拦截成功");
                        return (IntPtr)1;
                    }
                    if (wParam == (IntPtr)WM_SYSKEYDOWN &&
                        vkCode == (int)Keys.F4 &&
                        (GetAsyncKeyState(VK_MENU) & 0x8000) != 0)
                    {
                        Console.WriteLine("Alt+Tab拦截成功");
                        return (IntPtr)1;
                    }

                    // 其他锁屏状态下的拦截逻辑...
                }
                else // 非锁屏状态逻辑
                {
                    // 检测 Win + L 组合键
                    if (vkCode == (int)Keys.L && IsWinKeyPressed() &&
                        wParam == (IntPtr)WM_KEYDOWN)
                    {
                        // 防抖逻辑
                        if ((DateTime.Now - _lastTriggerTime).TotalMilliseconds < 500)
                        {
                            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
                        }
                        _lastTriggerTime = DateTime.Now;

                        // 清理按键状态
                        //ReleaseAllModifierKeys();
                         OnWinLDetected?.Invoke(); 
                        // 触发自定义锁屏（不再调用系统锁屏）
                        
                        return (IntPtr)1; // 必须返回1以阻止系统处理
                    }
                }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private static bool IsWinKeyPressed()
        {
            Console.WriteLine("win键被按");
            return (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 || (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0;
        }
        private static bool IsAltKeyPressed()
        {
            Console.WriteLine("alt键被按");
            return (GetAsyncKeyState(VK_LMENU) & 0x8000) != 0;
        }
        private static bool IsOtherKeyPressed()
        {
            for (int key = 0; key < 256; key++)
            {
                if (key != VK_LWIN && key != VK_RWIN &&
                    (GetAsyncKeyState(key) & 0x8000) != 0)
                {
                    return true;
                }
            }
            return false;
        }
        private static void ReleaseAllModifierKeys()
        {
            const int KEYEVENTF_KEYUP = 0x0002;
            keybd_event((byte)VK_LWIN, 0, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)VK_RWIN, 0, KEYEVENTF_KEYUP, 0);
            keybd_event((byte)Keys.L, 0, KEYEVENTF_KEYUP, 0);
        }
        public static void SetLockScreenState(bool isActive,bool isusewin)
        {
            _isLockScreenActive = isActive;
            _isUsewin=isusewin;
        }
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }


}