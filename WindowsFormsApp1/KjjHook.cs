using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace KeyHook
{
    public class HotKeyManager : IDisposable
    {
        // Win32 API声明
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 常量定义
        private const uint MOD_WIN = 0x0001;
        private const uint VK_L = 0x4C;
        private const int HOTKEY_ID = 0x3000;

        private IntPtr _windowHandle;
        private bool _disposed;
        private HotKeyMessageFilter _messageFilter; // 添加成员变量

        public event Action HotKeyPressed;

        public HotKeyManager(IntPtr hWnd)
        {
            _windowHandle = hWnd;
            Register();
        }

        private void Register()
        {
            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_WIN, VK_L))
            {
                throw new ApplicationException("热键注册失败，可能需要管理员权限");
            }

            // 保存过滤器实例并添加
            _messageFilter = new HotKeyMessageFilter(this);
            Application.AddMessageFilter(_messageFilter);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Application.RemoveMessageFilter(_messageFilter); // 使用成员变量
                }
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _disposed = true;
            }
        }

        private class HotKeyMessageFilter : IMessageFilter
        {
            private const int WM_HOTKEY = 0x0312;
            private readonly HotKeyManager _parent;

            public HotKeyMessageFilter(HotKeyManager parent)
            {
                _parent = parent;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_HOTKEY && (int)m.WParam == HOTKEY_ID)
                {
                    _parent.OnHotKeyPressed();
                    return true;
                }
                return false;
            }
        }

        private void OnHotKeyPressed()
        {
            HotKeyPressed?.Invoke();
        }
    }

    public class LockKeyboardHook : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string lpModuleName); // 添加声明

        private const uint WH_KEYBOARD_LL = 13;
        private IntPtr _hookHandle;
        private readonly LowLevelKeyboardProc _hookProc;
        private bool _isLocked;

        public LockKeyboardHook()
        {
            _hookProc = HookCallback;
            SetHook();
        }

        private void SetHook()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                // 使用正确的GetModuleHandle声明
                _hookHandle = SetWindowsHookEx(
                    (int)WH_KEYBOARD_LL,
                    _hookProc,
                    GetModuleHandle(curModule.ModuleName),
                    0
                );
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _isLocked)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (IsWinKey(vkCode)) return (IntPtr)1;
                if (IsF4Key(vkCode)) return (IntPtr)1;
                if (IsTabKey(vkCode)) return (IntPtr)1;

                //if (IsAltCombination(vkCode, wParam)) return (IntPtr)1;
            }
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private bool IsWinKey(int vkCode) =>
            vkCode == 0x5B || vkCode == 0x5C;
        private bool IsF4Key(int vkCode) =>
            vkCode == 0x73;
        private bool IsTabKey(int vkCode) =>
            vkCode == 0x09;

        private bool IsAltCombination(int vkCode, IntPtr wParam)
        {
            const int VK_MENU = 0x12;
            bool altPressed = (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;

            return altPressed && (
                (vkCode == 0x73 && wParam == (IntPtr)0x0104) || // Alt+F4
                (vkCode == 0x09 && wParam == (IntPtr)0x0104)    // Alt+Tab
            );
        }

        public void SetLockState(bool isLocked)
        {
            _isLocked = isLocked;
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookHandle);
        }

        #region Win32 API
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk); // 补充声明

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam); // 补充声明

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        #endregion
    }
}
