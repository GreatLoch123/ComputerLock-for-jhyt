using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Threading.Timer;
namespace ComputerLock.Hooks
{
    public class UserActivityMonitor : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool GetLastInputInfo(ref LastInputInfo plii);

        private struct LastInputInfo
        {
            public uint cbSize;
            public uint dwTime;
        }

        private Timer _timer;
        private int _autoLockMilliseconds;
        private int _isMonitoring; // 0:停止监控, 1:正在监控
        private readonly ISynchronizeInvoke _syncObject;

        public event EventHandler OnIdle;

        public UserActivityMonitor(ISynchronizeInvoke syncObject = null)
        {
            _syncObject = syncObject ?? new WindowsFormsSynchronizationProvider();
        }

        public void Initialize(int idleSeconds)
        {
            if (idleSeconds <= 0)
                throw new ArgumentException("空闲时间必须大于0秒");

            _autoLockMilliseconds = idleSeconds * 1000;
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, 5000);
        }

        public void Start()
        {
            if (Interlocked.CompareExchange(ref _isMonitoring, 1, 0) == 0)
            {
                _timer.Change(0, 5000);
                DebugLog("开始用户活动监控");
            }
        }

        public void Stop()
        {
            if (Interlocked.CompareExchange(ref _isMonitoring, 0, 1) == 1)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                DebugLog("停止用户活动监控");
            }
        }

        private void TimerCallback(object state)
        {
            if (Interlocked.CompareExchange(ref _isMonitoring, 0, 0) == 0)
                return;

            try
            {
                var lastInput = new LastInputInfo
                {
                    cbSize = (uint)Marshal.SizeOf(typeof(LastInputInfo))
                };

                if (!GetLastInputInfo(ref lastInput)) return;

                // 处理TickCount溢出（约49.7天循环一次）
                var elapsed = (Environment.TickCount - (int)lastInput.dwTime) & 0x7FFFFFFF;

                if (elapsed > _autoLockMilliseconds)
                {
                    DebugLog($"检测到系统空闲：{elapsed}ms");
                    Stop();

                    // 使用异步委托触发事件
                    var handler = OnIdle;
                    if (handler != null)
                    {
                        _syncObject.BeginInvoke(new Action(() =>
                        {
                            try
                            {
                                handler(this, EventArgs.Empty);
                            }
                            catch (Exception ex)
                            {
                                DebugLog($"事件处理异常：{ex.Message}");
                            }
                        }), null);
                    }
                }
            }
            catch (Exception ex)
            {
                DebugLog($"定时器回调异常：{ex.Message}");
            }
        }

        public void Dispose()
        {
            Stop();
            _timer?.Dispose();
            DebugLog("监控资源已释放");
        }

        private static void DebugLog(string message)
        {
#if DEBUG
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
#endif
        }

        // Windows Forms同步上下文包装器
        private class WindowsFormsSynchronizationProvider : ISynchronizeInvoke
        {
            public IAsyncResult BeginInvoke(Delegate method, object[] args)
            {
                if (Application.OpenForms.Count > 0)
                {
                    return Application.OpenForms[0].BeginInvoke(method, args);
                }
                return method.Method.Invoke(method.Target, args) as IAsyncResult;
            }

            public object EndInvoke(IAsyncResult result)
            {
                return result.AsyncState;
            }

            public object Invoke(Delegate method, object[] args)
            {
                if (Application.OpenForms.Count > 0)
                {
                    return Application.OpenForms[0].Invoke(method, args);
                }
                return method.Method.Invoke(method.Target, args);
            }

            public bool InvokeRequired => Application.OpenForms.Count > 0 && Application.OpenForms[0].InvokeRequired;
        }
    }
}