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
    public partial class Form3 : Form
    {
        //调用ConfigManagere的LoadConfig方法丢失config里的数据
        private static LockScreenConfig config = ConfigManager.LoadConfig();
        public Form3()
        {
            InitializeComponent();
        }

        private void metroTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine(config.Password);
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChangePassword();
        }
        private void ChangePassword()
        {
            if (metroTextBox1.Text == config.Password )
            {
                if (metroTextBox2.Text != null && metroTextBox2.Text == metroTextBox3.Text)
                {
                    config.Password = metroTextBox2.Text;
                    ConfigManager.SaveConfig(config); //调用ConfigManger的SaveConfig方法来保存新设置的文件
                    DialogResult result =MessageBox.Show("密码已经修改成功，请牢记新密码，如果出现无法打开的情况请联系信息科");
                    Application.Restart();
                }
                else { MessageBox.Show("请确认两次输入的新密码一致"); }
            }
            
            else { MessageBox.Show("旧密码错误，请重新输入"); }
        }
        private void metroTextBox3_Click(object sender, EventArgs e)
        {

        }

        private void metroTextBox2_Click(object sender, EventArgs e)
        {

        }

        private void metroTextBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Dispose();
        }
    }
}
