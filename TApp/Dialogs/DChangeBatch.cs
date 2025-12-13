using MTs.Auditrails;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public string logPath = Path.Combine(
               Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
               "MTE",
               "Logs",
               "ALL",
               "CB.ptl"
           );

        LogHelper<e_LogType> Logger { get; set; }

        public UserData CurrentUser { get; set; } = null;
        public DChangeBatch()
        {
            InitializeComponent();
            Logger = new LogHelper<e_LogType>(logPath);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng hủy thay đổi số lô sản xuất từ '{BatchCode}' sang '{ipBatch.Text.Trim()}'", "Đổi số lô sản xuất");
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void uiTableLayoutPanel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ipBatch_DoubleClick(object sender, EventArgs e)
        {
            Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng mở hộp thoại nhập số lô sản xuất", "Đổi số lô sản xuất");

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
            Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng mở hộp thoại nhập vã vạch (barcode)", "Đổi số lô sản xuất");
            if (!adminMode)
            {
                this.ShowErrorDialog("Chế độ nhập vã vạch (barcode) chỉ dành cho quản trị viên.");
                return;
            }

            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập vã vạch (barcode)";
                enterText.TextValue = ipBarcode.Text;
                enterText.IsPassword = false; // Thiết lập chế độ nhập mật khẩu
                enterText.EnterClicked += (s, args) =>
                {
                    ipBarcode.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //ghi log thay đổi lô
            Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng thay đổi số lô sản xuất từ '{BatchCode}' sang '{ipBatch.Text.Trim()}' với Barcode {ipBarcode.Text.Trim()}", "Đổi số lô sản xuất");
            string ruleTemplate = AppConfigs.Current.Batch_Rule_Template;
            if (string.IsNullOrWhiteSpace(ruleTemplate))
            {
                this.ShowErrorDialog("Chưa có quy tắc định dạng mã số lô, vui lòng liên hệ quản trị viên hệ thống để được hỗ trợ.");
                return;
            }
            //kiểm tra trước khi lưu
            if (!IsValid(ipBatch.Text.Trim(),ruleTemplate, int.Parse(AppConfigs.Current.Line_Name!.Split(' ').Last())))
            {
                this.ShowErrorDialog("Mã số lô không hợp lệ, vui lòng kiểm tra lại định dạng.");
                return;
            }

            BatchCode = ipBatch.Text.Trim();
            Barcode = ipBarcode.Text.Trim();
            DialogResult = DialogResult.OK;
        }

        bool IsValid(string input, string ruleTemplate, int line)
        {
            string regex = ruleTemplate.Replace("{LINE}", line.ToString());

            // 1. Replace DATE
            var dateFormats = new List<string>();

            regex = Regex.Replace(regex,
                @"\{DATE:([a-zA-Z]+)\}",
                m =>
                {
                    string format = m.Groups[1].Value;
                    dateFormats.Add(format);

                    int len = format.Length;
                    return $@"(?<DATE>\d{{{len}}})";
                });

            // 2. Replace A / N / AN
            regex = Regex.Replace(regex,
                @"\{(A|N|AN):(\d+)(,\d+)?\}",
                m =>
                {
                    string type = m.Groups[1].Value;
                    string min = m.Groups[2].Value;
                    string max = m.Groups[3].Success
                        ? m.Groups[3].Value.TrimStart(',')
                        : min;

                    string range = min == max ? min : $"{min},{max}";

                    return type switch
                    {
                        "A" => $@"[A-Za-z]{{{range}}}",
                        "N" => $@"\d{{{range}}}",
                        "AN" => $@"[A-Za-z0-9]{{{range}}}",
                        _ => ""
                    };
                });

            regex = "^" + regex + "$";

            var match = Regex.Match(input, regex);
            if (!match.Success) return false;

            // 3. Validate DATE thật
            foreach (string fmt in dateFormats)
            {
                if (!DateTime.TryParseExact(
                        match.Groups["DATE"].Value,
                        fmt,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _))
                    return false;
            }

            return true;
        }

        string? data_file_path { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "TanTien", "Users", "users.database");
        private void btnedit_Click(object sender, EventArgs e)
        {
            Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng mở chế độ chỉnh sửa số lô sản xuất", "Đổi số lô sản xuất");
            if (CurrentUser.Role != "Admin")
            {
                bool Is2FA = UserHelper.Validate2FA(ipUser.Text, ip2FACode.Text.Trim(), data_file_path, 3);
                bool IsAdmin = UserHelper.IsAdmin(ipUser.Text, data_file_path);

                if (!IsAdmin)
                {
                    this.ShowErrorDialog("Tài khoản bạn nhập không có quyền thay đổi số lô sản xuất, vui lòng liên hệ quản trị viên hệ thống để được hỗ trợ.");
                    Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng '{ipUser.Text}' không có quyền thay đổi số lô sản xuất", "Đổi số lô sản xuất");
                    return;
                }

                if (!Is2FA)
                {
                    this.ShowErrorDialog("Mã xác thực 2FA không chính xác, vui lòng kiểm tra lại.");
                    Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng nhập '{ipUser.Text}' và nhập mã 2FA không chính xác khi thay đổi số lô sản xuất", "Đổi số lô sản xuất");
                    return;
                }

                // Nếu người dùng là quản trị viên và mã 2FA hợp lệ, cho phép thay đổi số lô

                ipBatch.Enabled = true;
                ipBatch.DropDownStyle = UIDropDownStyle.DropDown;
                ipBatch.FillColor = Color.Yellow;

                ipBarcode.Enabled = true;
                ipBarcode.FillColor = Color.Yellow;
                btnedit.Enabled = false;

                Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng đã nhập '{ipUser.Text}' đã xác thực thành công và được phép thay đổi số lô sản xuất", "Đổi số lô sản xuất");
            }
            else
            {

            }
        }

        private void DChangeBatch_Load(object sender, EventArgs e)
        {
            ipBatch.Text = BatchCode;
            ipBarcode.Text = Barcode;
            if (CurrentUser.Role == "Admin")
            {
                adminMode = true;
                ipBatch.DropDownStyle = UIDropDownStyle.DropDownList;
                ipBarcode.Enabled = true;
                btnedit.Enabled = false;
            }
            else
            {
                adminMode = false;
                ipBatch.DropDownStyle |= UIDropDownStyle.DropDown;
                ipBarcode.Enabled = false;
                btnedit.Enabled = true;
            }

        }

        private void ipBatch_SelectedIndexChanged(object sender, EventArgs e)
        {
            Logger.WriteLogAsync(CurrentUser.Username, e_LogType.UserAction, $"Người dùng chọn số lô sản xuất '{ipBatch.SelectedItem}' từ danh sách", "Đổi số lô sản xuất");
            //lấy barcode từ dic bt
            if (ipBatch.SelectedItem is not null)
            {
                string selectedBatch = ipBatch.SelectedItem.ToString().Split("-")[0];
                if (bt.TryGetValue(selectedBatch, out string barcode))
                {
                    ipBarcode.Text = barcode;
                }
                else
                {
                    ipBarcode.Text = "00000";

                }
            }

        }

        private void ipUser_DoubleClick(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập tên Quản Trị";
                enterText.TextValue = ipUser.Text;
                enterText.EnterClicked += (s, args) =>
                {
                    ipUser.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }
    }
}
