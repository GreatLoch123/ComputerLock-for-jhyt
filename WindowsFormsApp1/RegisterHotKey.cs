using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace KeyHook
{
    public class SecureHotKey : IDisposable
    {
        // Win32 API声明
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // 常量定义
        private const uint MOD_ALT = 0x0001;
        private const uint VK_L = 0x4C;
        private const int HOTKEY_ID = 0x3000;

        private IntPtr _windowHandle;
        private bool _disposed = false;

        public SecureHotKey(IntPtr hWnd)
        {
            _windowHandle = hWnd;
            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_ALT, VK_L))
            {
                throw new ApplicationException("热键注册失败，可能需要管理员权限");
            }

            // 注册消息过滤器（.NET 3.5兼容写法）
            Application.AddMessageFilter(new HotKeyMessageFilter(this));
        }

        // 实现IDisposable接口（兼容.NET 3.5）
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
                    // 释放托管资源
                    Application.RemoveMessageFilter(_messageFilter);
                }
                // 释放非托管资源
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _disposed = true;
            }
        }

        // 消息过滤器类
        private class HotKeyMessageFilter : IMessageFilter
        {
            private readonly SecureHotKey _parent;

            public HotKeyMessageFilter(SecureHotKey parent)
            {
                _parent = parent;
            }

            public bool PreFilterMessage(ref Message m)
            {
                const int WM_HOTKEY = 0x0312;
                if (m.Msg == WM_HOTKEY && (int)m.WParam == HOTKEY_ID)
                {
                    // 触发事件
                    if ( HotKeyPressed != null)
                    {
                        HotKeyPressed(_parent, EventArgs.Empty);
                    }
                    return true; // 阻止系统处理
                }
                return false;
            }
        }

        // 兼容.NET 3.5的事件声明
        public static event EventHandler HotKeyPressed;

        // 静态实例管理（防止GC回收）
        private static HotKeyMessageFilter _messageFilter;

        // 修改后的注册逻辑
        public static SecureHotKey Create(IntPtr hWnd)
        {
            var instance = new SecureHotKey(hWnd);
            _messageFilter = new HotKeyMessageFilter(instance);
            return instance;
        }
    }
}