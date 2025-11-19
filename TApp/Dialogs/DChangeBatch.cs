using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TApp.Configs;
using TApp.Infrastructure;
using TApp.Views.Dashboard;
using TTManager.Auth;
using TTManager.Diaglogs;
using TTManager.Masan;

namespace TApp.Dialogs
{
    public partial class DChangeBatch : Form
    {
        public string BatchCode { get; set; } = "NNN";
        public string Barcode { get; set; } = "000";

        private bool adminMode = false; // Biến để kiểm tra chế độ quản trị viên

        public Dictionary<string, string> bt = new Dictionary<string, string>();

        public UserData CurrentUser { get; set; } = null;
        public DChangeBatch()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void uiTableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ipBatch_DoubleClick(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập Số Lô";
                enterText.TextValue = ipBatch.Text;
                enterText.IsPassword = false; // Thiết lập chế độ nhập mật khẩu
                enterText.EnterClicked += (s, args) =>
                {
                    ipBatch.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }

        private void uiTextBox1_DoubleClick(object sender, EventArgs e)
        {
            if(!adminMode)
            {
                this.ShowErrorDialog("Chế độ nhập vã vạch (barcode) chỉ dành cho quản trị viên.");
                return;
            }

            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập vã vạch (barcode)";
                enterText.TextValue = uiTextBox1.Text;
                enterText.IsPassword = false; // Thiết lập chế độ nhập mật khẩu
                enterText.EnterClicked += (s, args) =>
                {
                    uiTextBox1.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            BatchCode = ipBatch.Text.Trim();
            Barcode = uiTextBox1.Text.Trim();
            DialogResult = DialogResult.OK;
        }
        string? data_file_path { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "TanTien", "Users", "users.database");
        private void btnedit_Click(object sender, EventArgs e)
        {
            if (CurrentUser.Role != "Admin")
            {
                bool Is2FA = UserHelper.Validate2FA(ipUser.Text, ip2FACode.Text.Trim(), data_file_path);
                bool IsAdmin = UserHelper.IsAdmin(ipUser.Text, data_file_path);

                if (!IsAdmin)
                {
                    this.ShowErrorDialog("Tài khoản bạn nhập không có quyền thay đổi số lô sản xuất, vui lòng liên hệ quản trị viên hệ thống để được hỗ trợ.");
                    return;
                }

                if (!Is2FA)
                {
                    this.ShowErrorDialog("Mã xác thực 2FA không chính xác, vui lòng kiểm tra lại.");
                    return;
                }

                // Nếu người dùng là quản trị viên và mã 2FA hợp lệ, cho phép thay đổi số lô

                ipBatch.Enabled = true;
                ipBatch.DropDownStyle = UIDropDownStyle.DropDown;
                ipBatch.FillColor = Color.Yellow;

            }
            else
            {

            }
        }

        private void DChangeBatch_Load(object sender, EventArgs e)
        {
            ipBatch.Text = BatchCode;
            uiTextBox1.Text = Barcode;
            if(CurrentUser.Role == "Admin")
            {
                adminMode = true;
                ipBatch.DropDownStyle = UIDropDownStyle.DropDownList;
                uiTextBox1.Enabled = true;
                btnedit.Enabled = false;
            }
            else
            {
                adminMode = false;
                ipBatch.DropDownStyle |= UIDropDownStyle.DropDown;
                uiTextBox1.Enabled = false;
                btnedit.Enabled = true;
            }

        }

        private void ipBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            //lấy barcode từ dic bt
            if (ipBatch.SelectedItem is not null)
            {
                string selectedBatch = ipBatch.SelectedItem.ToString().Split("-")[0];
               if(bt.TryGetValue(selectedBatch, out string barcode))
               {
                    uiTextBox1.Text = barcode;
               }
                else
                {
                    uiTextBox1.Text = "00000";
                    
                }
            }
            
        }
    }
}
