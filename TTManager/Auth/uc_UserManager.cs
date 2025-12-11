
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTManager.Diaglogs;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TTManager.Auth
{
    public partial class uc_UserManager : UIUserControl
    {
        public uc_UserManager()
        {
            InitializeComponent();
        }
        public string CurrentUserName { get; set; } = string.Empty; // Biến để lưu tên người dùng hiện tại
        private DataTable userList;
        public event EventHandler<LoginActionEventArgs> OnAction; // Sự kiện đăng nhập, đăng xuất, cập nhật thông tin người dùng, v.v.
        public bool IS2FAEnabled { get; set; } = false; // Biến để kiểm tra xem 2FA có được bật hay không
        // Đường dẫn đến file dữ liệu SQLite, có thể thay đổi theo nhu cầu, mặc dịnh trong Appdata Local TanTien/Users
        public string data_file_path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TanTien", "Users", "users.database");
        // Biến để lưu log


        public void INIT()
        {
            // Initialize the user control, e.g., load user data, set up UI elements, etc.

        }

        private void AddActionButtons()
        {
            // Check tránh add trùng
            if (uiDataGridView1.Columns.Contains("btnDelete")) return;

            var btnDelete = new DataGridViewButtonColumn();
            btnDelete.Name = "btnDelete";
            btnDelete.HeaderText = "";
            btnDelete.Text = "Xóa";
            btnDelete.UseColumnTextForButtonValue = true;

            var btnShowUser = new DataGridViewButtonColumn();
            btnShowUser.Name = "btnShow2FA";
            btnShowUser.HeaderText = "";
            btnShowUser.Text = "Show2FA";
            btnShowUser.UseColumnTextForButtonValue = true;

            // Add lần lượt: Xóa – Xem user
            uiDataGridView1.Columns.Add(btnDelete);
            uiDataGridView1.Columns.Add(btnShowUser);
        }

        private void btnLoad_Click_1(object sender, EventArgs e)
        {
            btnLoad.Enabled = false; // Disable the button to prevent multiple clicks
            if (CurrentUserName == string.Empty)
            {
                OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Chưa đăng nhập" });
                btnLoad.Enabled = true; // Re-enable the button if not logged in
                return;
            }

            Task.Run(() =>
            {
                userList = UserData.GetUserListFromDB(data_file_path);
                this.Invoke(new Action(() =>
                {
                    uiDataGridView1.DataSource = userList;
                    AddActionButtons();
                }));
            });


            btnLoad.Enabled = true; // Re-enable the button after loading
        }

        private void uiDataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var colName = uiDataGridView1.Columns[e.ColumnIndex].Name;
                var row = uiDataGridView1.Rows[e.RowIndex];

                if (colName == "btnDelete")
                {
                    var id = row.Cells["Username"].Value?.ToString();
                    var confirm = MessageBox.Show($"Xóa tài khoản = {id}?", "Xác nhận", MessageBoxButtons.YesNo);
                    if (confirm == DialogResult.Yes)
                    {
                        if (CurrentUserName == id)
                        {
                            OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Không thể xóa tài khoản đang đăng nhập" });
                            return;
                        }
                        if (UserData.DeleteUser(id, data_file_path))
                        {

                            OnAction?.Invoke(this, new LoginActionEventArgs { Status = true, Message = $"Đã xóa tài khoản {id}" });
                            // Refresh the user list
                            btnLoad_Click_1(sender, e);
                        }
                        else
                        {
                            OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = $"Không thể xóa tài khoản {id}" });

                        }
                    }
                }
                else if (colName == "btnShow2FA")
                {
                    if (ipPassword.Text.Trim() == string.Empty)
                    {
                        OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Vui lòng nhập mật khẩu để xem mã của người dùng khác" });
                        return;
                    }

                    if (IS2FAEnabled)
                    {
                        if (ipTwoFA.Text.Trim() == string.Empty)
                        {
                            OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Vui lòng nhập mã 2FA để xem mã của người dùng khác" });
                            return;
                        }
                        if (!UserHelper.Validate2FA(CurrentUserName, ipTwoFA.Text.Trim(), data_file_path))
                        {
                            OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Mã 2FA không đúng!" });
                            return;
                        }
                    }

                    if (!UserHelper.ValidateCredentials(CurrentUserName, ipPassword.Text.Trim(), data_file_path))
                    {
                        //ghi log thất bại
                        OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Sai mật khẩu!" });
                        return;
                    }


                    UserData userData = UserData.GetUserByUsername(row.Cells["Username"].Value?.ToString(), data_file_path);

                    if (userData == null || string.IsNullOrWhiteSpace(userData.Username))
                    {
                        OnAction?.Invoke(this, new LoginActionEventArgs
                        {
                            Status = false,
                            Message = "Không tìm thấy thông tin người dùng để tạo mã QR."
                        });
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(userData.Key2FA))
                    {
                        OnAction?.Invoke(this, new LoginActionEventArgs
                        {
                            Status = false,
                            Message = "Chưa có khóa 2FA cho tài khoản này."
                        });
                        return;
                    }

                    string issuer = "MSI-CZ :" + userData.Username;
                    string otpauth = TwoFAHelper.GenerateQrCodeUri(userData.Key2FA, userData.Username, issuer);

                    using (var dlg = new QrCodeDialog(otpauth, $"QR 2FA cho {userData.Username}"))
                    {
                        dlg.ShowDialog(FindForm());
                    }
                }


            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {

            //kiểm tra mật khẩu và mã OTP nếu 2FA được bật

            if (ipPassword.Text.Trim() == string.Empty)
            {
                OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Vui lòng nhập mật khẩu để xác thực thêm người dùng mới." });
                return;
            }

            if (IS2FAEnabled)
            {
                if (ipTwoFA.Text.Trim() == string.Empty)
                {
                    OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Vui lòng nhập mã 2FA để xác thực thêm người dùng mới." });
                    return;
                }
                if (!UserHelper.Validate2FA(CurrentUserName, ipTwoFA.Text.Trim(), data_file_path))
                {
                    OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Mã 2FA không đúng!" });
                    return;
                }
            }
            if (!UserHelper.ValidateCredentials(CurrentUserName, ipPassword.Text.Trim(), data_file_path))
            {
                //ghi log thất bại
                OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Sai mật khẩu!" });
                return;


            }
            using (EnterAddUser enterText = new EnterAddUser())
            {
                enterText.EnterClicked += (s, args) =>
                {
                    string username = enterText.TextValue;
                    string password = enterText.passwordValue;
                    e_User_Role role = enterText.UserRole;
                    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                    {
                        OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Tên người dùng hoặc mật khẩu không được để trống." });
                        return;
                    }

                    if (UserData.GetUserListFromDB(data_file_path).AsEnumerable().Any(row => row.Field<string>("Username") == username))
                    {
                        OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = "Tên người dùng đã tồn tại." });
                        return;
                    }

                    // Tạo người dùng mới
                    UserData newUser = new UserData
                    {
                        Username = username,
                        Password = password, // Mật khẩu sẽ được hash trong UserData
                        Role = role.ToString(),
                        Key2FA = string.Empty // Khóa 2FA có thể được thiết lập sau
                    };
                    // Lưu người dùng mới vào cơ sở dữ liệu
                    if (UserHelper.AddUser(username, password, role.ToString(), data_file_path))
                    {
                        OnAction?.Invoke(this, new LoginActionEventArgs { Status = true, Message = $"Đã thêm tài khoản {username} với vai trò {role}" });
                        // Refresh the user list
                        btnLoad_Click_1(sender, e);
                    }
                    else
                    {
                        OnAction?.Invoke(this, new LoginActionEventArgs { Status = false, Message = $"Không thể thêm tài khoản {username}" });
                    }
                };
                enterText.ShowDialog();
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {

        }

        private void ipPassword_DoubleClick(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập mật khẩu";
                enterText.TextValue = ipPassword.Text;
                enterText.EnterClicked += (s, args) =>
                {
                    ipPassword.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }
    }
}
    

