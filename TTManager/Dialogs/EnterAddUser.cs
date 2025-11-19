using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Sunny.UI;
using TTManager.Auth;

namespace TTManager.Diaglogs
{
    public partial class EnterAddUser : Form
    {
        public event EventHandler EnterClicked;
        public string TextValue { get; set; }
        public string passwordValue { get; set; } = string.Empty; // Biến để lưu giá trị mật khẩu
        public e_User_Role UserRole { get; set; } // Biến để lưu vai trò người dùng
        public string TileText { get; set; } = "Nhập văn bản";
        public EnterAddUser()
        {
            InitializeComponent();

        }

        private void uiSymbolButton1_Click(object sender, EventArgs e)
        {
            //// Gửi phím về MainForm
            TextValue = ipUsername.Text;
            passwordValue = ipPassword.Text; // Lưu giá trị mật khẩu
            if (ipRole.SelectedItem != null)
            {
                UserRole = (e_User_Role)Enum.Parse(typeof(e_User_Role), ipRole.SelectedItem.ToString());
            }
            else
            {
                UserRole = e_User_Role.Operator; // Mặc định là Operator nếu không có lựa chọn
            }
            EnterClicked?.Invoke(this, EventArgs.Empty);
            // Đóng form với kết quả OK
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Entertext_Load(object sender, EventArgs e)
        {
            ipUsername.Text = TextValue;
            //load enum vào comboBox1
            ipRole.Items.AddRange(Enum.GetNames(typeof(e_User_Role)));
            // Chọn mặc định là Worker
            ipRole.SelectedItem = e_User_Role.Operator.ToString();
            // Thiết lập tiêu đề của form
            uiTitlePanel1.Text = TileText;
        }


        private void ipPassword_DoubleClick(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập Mật Khẩu";
                enterText.TextValue = ipPassword.Text;
                enterText.IsPassword = true; // Thiết lập chế độ nhập mật khẩu
                enterText.EnterClicked += (s, args) =>
                {
                    ipPassword.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }

        private void ipUsername_DoubleClick(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập Tên Đăng Nhập";
                enterText.TextValue = ipUsername.Text;
                enterText.IsPassword = false; // Thiết lập chế độ nhập mật khẩu
                enterText.EnterClicked += (s, args) =>
                {
                    ipUsername.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }
    }
}
