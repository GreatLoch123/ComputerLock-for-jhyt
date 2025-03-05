using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public NotifyIcon yyIcon;
        private static LockScreenConfig config = ConfigManager.LoadConfig();
        public bool isinitializing = false;
        public bool isinitializing2 = false;

        public bool IsAutoStart = true;
        public bool IsRestore = true;
        public Form1()
        {
            InitializeComponent();
            this.pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox1.Image = System.Drawing.Image.FromFile(@"Resources/开.png");
            this.pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            this.pictureBox2.Image = System.Drawing.Image.FromFile(@"Resources/开.png");
            
            Locktime();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (Form3 ChangePassword = new Form3())
            {
                ChangePassword.ShowDialog();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }
        private void restart_confirm()
        {
            DialogResult result = MessageBox.Show(
                      "重启后修改生效，确认吗？",   // 提示信息
                      "操作确认",             // 标题
                      MessageBoxButtons.OKCancel, // 按钮类型
                      MessageBoxIcon.Question      // 图标
                  );

            // 判断用户点击的是否为"确定"
            if (result == DialogResult.OK)
            {
                // 点击确定后执行的代码
                Application.Restart();  // 示例：调用删除文件的方法
                                        // 或 RestartApplication();
            }
        }
        private void Locktime() 
        {
            isinitializing = true;
            isinitializing2 = true;
            this.metroComboBox1.SelectedIndex = ReturnMinits(config.LockTimeInSeconds);
            this.metroComboBox2.SelectedIndex = ReturnMinits(config.WallpaperChangeIntervalInSeconds);
            isinitializing = false;
            isinitializing2 = false;
        }
        private void Updatetime(int lockseconds,int flag)
        {
            if (flag == 0)
            {
                config.LockTimeInSeconds = lockseconds;
            }
            if (flag == 1) 
            {
                config.WallpaperChangeIntervalInSeconds = lockseconds;
            }
            ConfigManager.SaveConfig(config);
            Console.WriteLine("执行了更新");

        }
        private void Changeinitstat()
        {
            isinitializing=false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //自动启动
            if (IsAutoStart) 
            {
                this.pictureBox1.Image = System.Drawing.Image.FromFile(@"Resources/关.png");
                IsAutoStart = false;
                restart_confirm();

            }
            else
            {
                this.pictureBox1.Image = System.Drawing.Image.FromFile(@"Resources/开.png");
                IsAutoStart = true;
               restart_confirm();

            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            //恢复系统锁屏
            if (IsRestore)
            {
                this.pictureBox2.Image = System.Drawing.Image.FromFile(@"Resources/关.png");
                IsRestore = false;
                restart_confirm();

            }
            else
            {
                this.pictureBox2.Image = System.Drawing.Image.FromFile(@"Resources/开.png");
                IsRestore = true;
                restart_confirm();
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void metroComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            //锁屏图片切换更改时间
            if (!isinitializing)
            {
                if (this.metroComboBox1.SelectedIndex == 0)
                {
                    Updatetime(300,0);
                    restart_confirm();
                }
                if (this.metroComboBox1.SelectedIndex == 1)
                {
                    Updatetime(600, 0);
                    restart_confirm();

                }
                if (this.metroComboBox1.SelectedIndex == 2)
                {
                    Updatetime(18000, 0);
                    restart_confirm();
                }
            }
        }
        private int  ReturnMinits(int times)
        {
            if (times == 300)
            {
                return 0;
            }
            if (times == 600)
            {
                return 1;
            }
            if (times == 18000)
            {
                return 2;
            }
            return 0;
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // 在窗体关闭后释放资源
             this.Dispose();
            Console.WriteLine("资源已释放");

        } 
        private void metroComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //锁屏时间更改后执行
            if (!isinitializing2)
            {
                if (this.metroComboBox2.SelectedIndex == 0)
                {
                    Updatetime(300, 1);
                    restart_confirm();
                }
                if (this.metroComboBox2.SelectedIndex == 1)
                {
                    Updatetime(600, 1);
                    restart_confirm();
                }
                if (this.metroComboBox2.SelectedIndex == 2)
                {
                    Updatetime(18000, 1);
                    restart_confirm();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (Form4 ChangePassword = new Form4())
            {
                ChangePassword.ShowDialog();
            }
        }
    }
}
