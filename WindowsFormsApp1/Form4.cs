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
    public partial class Form4 : Form
    {
        private static LockScreenConfig config = ConfigManager.LoadConfig();
        public Form4()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //输入信息科内部密码可以更改密码
            if (metroTextBox1.Text =="xxk666") {
                config.Password = metroTextBox3.Text;
                ConfigManager.SaveConfig(config);
                MessageBox.Show("密码已经修改成功,请嘱咐牢记新密码");
                Application.Restart();
            }
            else
            {
                MessageBox.Show("信息科内部密码输入错误，请检查输入");
            }
        }
    }
}
