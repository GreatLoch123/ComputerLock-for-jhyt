using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ComputerLock
{
    /// <summary>
    /// WindowBlankScreen.xaml 的交互逻辑
    /// </summary>
    public partial class WindowBlankScreen : Window
    {
        private bool _isUnlock = false;
        public event EventHandler<EventArgs>? OnDeviceInput;

        private readonly AppSettings _appSettings;
        public WindowBlankScreen(AppSettings appSettings)
        {
            InitializeComponent();
            
            _appSettings = appSettings;
        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            string imagePath = "d34d8f5782e0193.jpg"; // 替换成实际图片路径
            // 加载图片并设置为背景
            Uri uri = new Uri(imagePath);
            BitmapImage bitmap = new BitmapImage(uri);
            imgBackground.Source = bitmap;
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
            OnDeviceInput?.Invoke(this, EventArgs.Empty);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isUnlock)
            {
                e.Cancel = true;
            }
        }
        public void Unlock()
        {
            _isUnlock = true;
        }
    }
}
