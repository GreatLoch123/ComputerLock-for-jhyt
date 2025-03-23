using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

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

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        // Virtual Key Codes
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_MENU = 0x12;
        private const int VK_F4 = 0x73;
        private const int VK_TAB = 0x09;
        private const int VK_L = 0x4C;

        private static IntPtr _hookHandle = IntPtr.Zero;
        private static LowLevelKeyboardProc _hookProc;
        private static DateTime _lastTriggerTime = DateTime.MinValue;

        // 状态标志
        private static bool _isLockScreenActive = false;
        private static bool _shouldBlockWinKey = true;

        public static event Action OnWinLDetected;

        public static void InstallHook()
        {
            _hookProc = HookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc,
                    GetModuleHandle(curModule.ModuleName), 0);
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

                // 锁屏状态拦截逻辑
                if (_isLockScreenActive)
                {
                    // 拦截所有Win键操作
                    //if (IsWinKey(vkCode))
                    //{
                    //    Console.WriteLine("拦截Win键");
                    //    //return (IntPtr)1;
                    //}
                    // 拦截Alt组合键
                    if (IsAltCombination(vkCode, wParam))
                    {
                        Console.WriteLine("拦截Alt组合键");
                        return (IntPtr)1;
                    }
                }
                //正常状态逻辑
                else
                {
                    // 检测Win+L组合键
                    if (IsWinLCombination(vkCode, wParam))
                    {
                        if (ThrottleEvent()) return (IntPtr)1;

                        OnWinLDetected?.Invoke();
                        return (IntPtr)1;
                    }
                }
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        #region 状态判断方法
        private static bool IsWinKey(int vkCode) =>
            vkCode == VK_LWIN || vkCode == VK_RWIN;

        private static bool IsAltCombination(int vkCode, IntPtr wParam)
        {
            // 检测Alt状态
            bool altPressed = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;

            return altPressed && (
                (vkCode == VK_F4 && wParam == (IntPtr)WM_SYSKEYDOWN) ||  // Alt+F4
                (vkCode == VK_TAB && wParam == (IntPtr)WM_SYSKEYDOWN)    // Alt+Tab
            );
        }

        private static bool IsWinLCombination(int vkCode, IntPtr wParam)
        {
            bool winPressed = (GetAsyncKeyState(VK_LWIN) & 0x8000) != 0 ||
                            (GetAsyncKeyState(VK_RWIN) & 0x8000) != 0;

            return vkCode == VK_L &&
                   winPressed &&
                   wParam == (IntPtr)WM_KEYDOWN;
        }
        #endregion

        #region 辅助方法
        private static bool ThrottleEvent()
        {
            // 500ms防抖
            if ((DateTime.Now - _lastTriggerTime).TotalMilliseconds < 500)
                return true;

            _lastTriggerTime = DateTime.Now;
            return false;
        }
        #endregion

        public static void SetLockScreenState(bool isActive)
        {
            _isLockScreenActive = isActive;
            // 锁屏时自动启用Win键拦截
            _shouldBlockWinKey = isActive;
        }

        // 添加以下方法用于安全释放资源
        public static void Dispose()
        {
            UninstallHook();
            _hookHandle = IntPtr.Zero;
        }
    }
}