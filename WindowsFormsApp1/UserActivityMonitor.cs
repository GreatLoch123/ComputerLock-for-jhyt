using System;
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

        struct LastInputInfo
        {
            public uint cbSize;
            public uint dwTime;
        }

        private Timer _timer;
        public EventHandler OnIdle;

        private int _autoLockMillisecond;
        private bool _isMonitoring = false;

        // 用于存储UI线程上下文
        private SynchronizationContext _syncContext;

        public void Init(int autoLockSecond)
        {
            _autoLockMillisecond = autoLockSecond * 1000;

            // 获取当前线程的同步上下文
            _syncContext = SynchronizationContext.Current;
            Console.WriteLine("定时器");
            // 使用 System.Threading.Timer 替代 System.Timers.Timer
            _timer = new Timer(TimerCallback, null, Timeout.Infinite, 1000); // 每秒触发一次
        }

        public void StartMonitoring()
        {
            _isMonitoring = true;
            _timer?.Change(0, 1000);
            Console.WriteLine("开始监控用户活动");
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            _timer?.Change(Timeout.Infinite, 1000);
            Console.WriteLine("停止监控用户活动");
        }

        // 定时器回调函数
        private void TimerCallback(object state)
        {

            if (!_isMonitoring)
            {
                Console.WriteLine("检测到用户空闲超过设定时间");

                return;
            }

            var lastInputInfo = new LastInputInfo();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

            if (GetLastInputInfo(ref lastInputInfo))
            {
                long elapsedMillisecond = Environment.TickCount - (int)lastInputInfo.dwTime;

                // TickCount 可能会溢出，需考虑这一点
                if (elapsedMillisecond > _autoLockMillisecond)
                {
                    Console.WriteLine("检测到用户空闲超过设定时间");

                    // 在 UI 线程上触发 OnIdle 事件
                    if (_syncContext != null)
                    {
                        Console.WriteLine("触发 OnIdle 事件");
                        _syncContext.Post(_ => OnIdle?.Invoke(this, EventArgs.Empty), null);
                    }
                    else
                    {
                        Console.WriteLine("同步上下文为空，无法触发事件");
                    }
                }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}