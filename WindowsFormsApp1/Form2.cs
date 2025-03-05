using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private static int currentImageIndex = 2;
        private static LockScreenConfig config = ConfigManager.LoadConfig();
        private Timer timer;
        private Image nextImage;

        public Form2()
        {
            InitializeComponent();
            DoubleBuffered = true; // 启用双缓冲以减少闪烁
            change_bz();
        }

        public void change_bz()
        {
            string imagePath = @"resources/3.png";
            timer = new Timer();
            timer.Interval = config.WallpaperChangeIntervalInSeconds * 1000;
            timer.Tick += (s, e) =>
            {
                UpdateWallpaper();
            };
            timer.Start();

            // 初始化窗口背景
            this.BackgroundImage = LoadImage(imagePath);
            this.BackgroundImageLayout = ImageLayout.Stretch;
            this.FormBorderStyle = FormBorderStyle.None;

            // 窗口最大化
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
        }

        private Image LoadImage(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
                using (MemoryStream ms = new MemoryStream(buffer))
                {
                    return Image.FromStream(ms);
                }
            }
        }

        private void UpdateWallpaper()
        {
            try
            {
                // 预加载下一张图片
                string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/" + $"{currentImageIndex}.png");
                nextImage = LoadImage(imagePath);

                // 更新背景图片
                if (this.BackgroundImage != null)
                {
                    var oldImage = this.BackgroundImage;
                    this.BackgroundImage = nextImage; // 设置为新图片
                    oldImage.Dispose(); // 释放旧图片资源
                }

                currentImageIndex++;
                if (currentImageIndex > 18)
                {
                    currentImageIndex = 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新壁纸时发生错误: {ex.Message}");
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Showpasswordbox();
        }

        public void Showpasswordbox()
        {
            using (Passwordbox passwordForm = new Passwordbox())
            {
                passwordForm.TopMost = true;
                if (passwordForm.ShowDialog() == DialogResult.OK)
                {
                    this.Close();
                }
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 停止计时器并释放资源
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }

            if (this.BackgroundImage != null)
            {
                this.BackgroundImage.Dispose();
                this.BackgroundImage = null;
            }

            if (nextImage != null)
            {
                nextImage.Dispose();
                nextImage = null;
            }

            Console.WriteLine("所有资源已释放");
        }
    }
}