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

        private void Locktime() 
        {
            isinitializing = true;
            this.metroComboBox1.SelectedIndex = ReturnMinits(config.LockTimeInSeconds);
            this.metroComboBox2.SelectedIndex = ReturnMinits(config.WallpaperChangeIntervalInSeconds);
            isinitializing = false;

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
            //if (flag == 0)
            //{
            //    ConfigManager.SaveConfig(new LockScreenConfig
            //    {
            //        LockTimeInSeconds = lockseconds,
            //    });
            //}
            //if (flag == 1)  
            //{
            //    ConfigManager.SaveConfig(new LockScreenConfig
            //    {
            //        WallpaperChangeIntervalInSeconds = lockseconds ,
            //    });
            //}

        }
        private void Changeinitstat()
        {
            isinitializing=false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (IsAutoStart) 
            {
                this.pictureBox1.Image = System.Drawing.Image.FromFile(@"Resources/关.png");
                IsAutoStart = false; 
            }
            else
            {
                this.pictureBox1.Image = System.Drawing.Image.FromFile(@"Resources/开.png");
                IsAutoStart = true;
            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (IsRestore)
            {
                this.pictureBox2.Image = System.Drawing.Image.FromFile(@"Resources/关.png");
                IsRestore = false;
            }
            else
            {
                this.pictureBox2.Image = System.Drawing.Image.FromFile(@"Resources/开.png");
                IsRestore = true;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void metroComboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (!isinitializing)
            {
                if (this.metroComboBox1.SelectedIndex == 0)
                {
                    Updatetime(300,0);
                }
                if (this.metroComboBox1.SelectedIndex == 1)
                {
                    Updatetime(600, 0);
                }
                if (this.metroComboBox1.SelectedIndex == 2)
                {
                    Updatetime(18000, 0);
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
            if (!isinitializing2)
            {
                if (this.metroComboBox2.SelectedIndex == 0)
                {
                    Updatetime(300, 1);
                }
                if (this.metroComboBox2.SelectedIndex == 1)
                {
                    Updatetime(600, 1);
                }
                if (this.metroComboBox2.SelectedIndex == 2)
                {
                    Updatetime(18000, 1);
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
