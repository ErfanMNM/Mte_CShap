

using Microsoft.VisualBasic.Logging;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using TTManager.Audit;
using TTManager.Diaglogs;
using TTManager.Helpers;

namespace TTManager.Auth
{
    public partial class ucLogin : UserControl
    {
        public ucLogin()
        {
            InitializeComponent();
        }

        public event EventHandler<LoginActionEventArgs>? OnLoginAction; // Sự kiện đăng nhập, đăng xuất, cập nhật thông tin người dùng, v.v.
        public bool IS2FAEnabled { get; set; } = true; // Biến để kiểm tra xem 2FA có được bật hay không
        // Đường dẫn đến file dữ liệu SQLite, có thể thay đổi theo nhu cầu, mặc dịnh trong Appdata Local TanTien/Users
        [Category("Data")]
        [Description("Đường dẫn đến file SQLite user data")]
        [Editor(typeof(FilePathEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string? data_file_path { get; set; } =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "TanTien", "Users", "users.database");


        private LogHelper<LoginAction> log; // Biến để lưu trữ thông tin log
        public UserData CurrentUser { get; private set; } // Biến để lưu trữ thông tin người dùng hiện tại

        DataTable UsersList = new DataTable();
        public void INIT()
        {
            //tạo đường dẫn đến Appdata Local
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //tạo đường dẫn đến file lưu log
            string logFilePath = Path.Combine(appDataPath, "TanTien", "Logs", "userlog.logs");
            //kiểm tra thư mục có tồn tại không
            string? directoryPath = Path.GetDirectoryName(data_file_path);
            if (!Directory.Exists(directoryPath))
            {

                //nếu không tồn tại thì tạo mới
                Directory.CreateDirectory(directoryPath);
            }
            //tạo thông tin log
            log = new LogHelper<LoginAction>(logFilePath);
            //kiểm tra thư mục có tồn tại không

            directoryPath = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(directoryPath))
            {
                //nếu không tồn tại thì tạo mới
                Directory.CreateDirectory(directoryPath);
            }

            //kiểm tra file dữ liệu có tồn tại không
            if (!File.Exists(data_file_path))
            {
                //nếu không tồn tại thì tạo mới
                SQLiteConnection.CreateFile(data_file_path);
                //tạo bảng users
                using (var conn = new SQLiteConnection($"Data Source={data_file_path};Version=3;"))
                {
                    conn.Open();
                    string sql = $@"CREATE TABLE ""users""(
                                                    ""ID""    INTEGER,
                                                    ""Username""  TEXT NOT NULL,
                                                    ""Password""  TEXT NOT NULL,
                                                    ""Salt""  TEXT NOT NULL,
                                                    ""Role""  TEXT NOT NULL,
                                                    ""Key2FA""    TEXT NOT NULL,
                                                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                            );";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }

            // Tạo DataTable để lưu danh sách người dùng
            UsersList = UserData.GetUserListFromDB(data_file_path);

            //thêm vào cbbox ipUserName
            ipUserName.Items.Clear();
            foreach (DataRow row in UsersList.Rows)
            {
                string username = row["Username"].ToString();
                if (!string.IsNullOrEmpty(username))
                {
                    ipUserName.Items.Add(username);
                }
            }

            if (ipUserName.Items.Count > 0)
            {
                // Chọn mục đầu tiên nếu có
                ipUserName.SelectedIndex = 0;
            }
            else
            {
                // Hiển thị thông báo nếu không có người dùng nào
                OnLoginAction?.Invoke(this, new LoginActionEventArgs
                {
                    Status = false,
                    Message = "Không có người dùng nào trong hệ thống."
                });
            }

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                btnLogin.Enabled = false; // Vô hiệu hóa nút đăng nhập trong quá trình xử lý
                btnLogin.Text = "Đang tải..."; // Thay đổi văn bản nút đăng nhập để hiển thị trạng thái
                // Kiểm tra thông tin đăng nhập
                string username = ipUserName.Text.Trim();
                string password = ipPassword.Text.Trim();

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    //trả về thông báo lỗi
                    OnLoginAction?.Invoke(this, new LoginActionEventArgs
                    {
                        Status = false,
                        Message = "Tên đăng nhập hoặc mật khẩu không được để trống."
                    });
                    return;
                }

                //kiểm tra thông tin đăng nhập trong sqlite abcc.bcaa
                if (UserHelper.ValidateCredentials(username, password, data_file_path))
                {
                    //lấy thông tin user từ sqlite
                    UserData dbUser = UserData.GetUserByUsername(username, data_file_path);
                    //kiểm tra có khóa 2FA không
                    if (IS2FAEnabled)
                    {
                        bool isValid = TwoFAHelper.VerifyOTP(dbUser.Key2FA, ipTwoFA.Text, digits: 6);
                        if (!isValid)
                        {
                            //ghi log, trả sự kiện
                            OnLoginAction?.Invoke(this, new LoginActionEventArgs
                            {
                                Status = false,
                                Message = "Mã OTP không hợp lệ."
                            });

                            //ghi log lỗi
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await log.WriteLogAsync("NA", LoginAction.Login, $"Đăng nhập thất bại cho người dùng {username}: Mã OTP không hợp lệ.");
                                }
                                catch (Exception ex)
                                {
                                    // log hoặc ignore
                                    Console.WriteLine($"Lỗi ghi log: {ex.Message}");
                                }
                            });

                            return;
                        }
                    }
                    //trả lại thông tin user
                    CurrentUser = dbUser;
                    //ghi log
                    Task.Run(async () =>
                    {
                        try
                        {
                            await log.WriteLogAsync(username, LoginAction.Login, $"Đăng nhập thành công cho người dùng {username}.");
                        }
                        catch (Exception ex)
                        {
                            // log hoặc ignore
                            Console.WriteLine($"Lỗi ghi log: {ex.Message}");
                        }
                    });
                    //await log.WriteLogAsync(username, LoginAction.Login, $"Đăng nhập thành công cho người dùng {username}.");

                    //trả về sự kiện
                    OnLoginAction?.Invoke(this, new LoginActionEventArgs
                    {
                        Status = true,
                        Message = $"Đăng nhập thành công cho người dùng {username}."
                    });

                    return;
                }
                else
                {
                    OnLoginAction?.Invoke(this, new LoginActionEventArgs
                    {
                        Status = false,
                        Message = "Tên đăng nhập hoặc mật khẩu không đúng."
                    });
                    //ghi log lỗi
                    Task.Run(async () =>
                    {
                        try
                        {
                            await log.WriteLogAsync("NA", LoginAction.Login, $"Đăng nhập thất bại cho người dùng {username}: Tên đăng nhập hoặc mật khẩu không đúng.");
                        }
                        catch (Exception ex)
                        {
                            // log hoặc ignore
                            Console.WriteLine($"Lỗi ghi log: {ex.Message}");
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                Task.Run(async () =>
                {
                    try
                    {
                        await log.WriteLogAsync("NA", LoginAction.Login, $"Lỗi khi đăng nhập: {ex.Message}");
                    }
                    catch (Exception logEx)
                    {
                        // log hoặc ignore
                        Console.WriteLine($"Lỗi ghi log: {logEx.Message}");
                    }
                });

                // Ghi log lỗi đăng nhập
                //await log.WriteLogAsync("NA", LoginAction.Login, $"Lỗi khi đăng nhập: {ex.Message}");
                // Trả về sự kiện lỗi
                OnLoginAction?.Invoke(this, new LoginActionEventArgs
                {
                    Status = false,
                    Message = $"Lỗi khi đăng nhập: {ex.Message}"
                });
            }
            finally
            {
                btnLogin.Enabled = true; // Bật lại nút đăng nhập sau khi xử lý xong
                btnLogin.Text = "Đăng nhập"; // Đặt lại văn bản nút đăng nhập
            }
        }

        private void ipPassword_DoubleClick(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập mật khẩu";
                enterText.TextValue = ipPassword.Text;
                enterText.IsPassword = true; // Thiết lập chế độ nhập mật khẩu
                enterText.EnterClicked += (s, args) =>
                {
                    ipPassword.Text = enterText.TextValue;
                };
                enterText.ShowDialog();
            }
        }
    }
}
