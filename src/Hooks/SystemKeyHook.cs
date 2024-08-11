using ComputerLock.Platforms;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;

namespace ComputerLock.Hooks
{
    public class SystemKeyHook : IDisposable
    {
   
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;
        private const int VK_L = 0x4C; // L 键
        private IntPtr _hookId = IntPtr.Zero;
        private readonly HookDelegate _hookCallback;
        private readonly ILocker _locker;
        private bool _winKeyPressed = false;
        private readonly LockService _lockService;
        public delegate IntPtr HookDelegate(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public SystemKeyHook(LockService lockService)
        {
            _hookCallback = KeyboardHookCallback;
            _lockService = lockService;
        }

        public void DisableSystemKey()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _hookCallback, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_KEYUP))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if (vkCode == VK_LWIN || vkCode == VK_RWIN)
                    {
                        _winKeyPressed = true;
                    }
                    else if (_winKeyPressed && vkCode == VK_L)
                    {
                        // 捕获 Win + L 组合键
                        _winKeyPressed = false;
                        CustomLockScreen();
                        return (IntPtr)1; // 阻止事件传递
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    if (vkCode == VK_LWIN || vkCode == VK_RWIN)
                    {
                        _winKeyPressed = false;
                    }
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private void CustomLockScreen()
        {
            _lockService.Lock();
            // TODO: 在这里调用你锁屏窗口的显示方法
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookId);
        }
    }
}
