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
    public partial class Passwordbox : Form
    {
        private TextBox passwordTextBox;
        public Passwordbox()
        {
            InitializeComponent();
            //this.Size = new System.Drawing.Size(300, 150);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "请输入密码：";
            // 添加控件
            //Label label = new Label { Text = "请输入密码：", Dock = DockStyle.Top, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            passwordTextBox = new TextBox { Dock = DockStyle.Top, PasswordChar = '*' };
            //this.Controls.Add(label);
            this.Controls.Add(passwordTextBox);

            // 绑定事件
            passwordTextBox.TextChanged += PasswordTextBox_TextChanged;
        }

        private void PasswordTextBox_TextChanged(object sender, EventArgs e)
        {
            LockScreenConfig config = ConfigManager.LoadConfig();

            // 实时检查密码
            if (passwordTextBox.Text == config.Password)
            {
                this.DialogResult = DialogResult.OK;
                //this.Hide();
            }
        }
    }
    
}
