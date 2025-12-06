using MTVS.Models;
using MTVS.Services;
using System.Reflection;
using Sunny.UI;

namespace MTVS
{
    public partial class MTMainForm : UIForm
    {
        private readonly ConfigService _configService;
        private AppwriteService? _appwriteService;
        private VersionManagerService? _versionManagerService;
        private ReleaseCheckResult? _currentUpdateCheck;
        private string? _currentBackupPath;

        public MTMainForm()
        {
            InitializeComponent();
            
            try
            {
                _configService = new ConfigService();
                InitializeServices();
                LoadCurrentVersion();
            }
            catch (Exception ex)
            {
                ShowError($"Khởi tạo thất bại: {ex.Message}");
            }
        }

        private void InitializeServices()
        {
            var config = _configService.GetConfig();
            var appwriteConfig = config.Appwrite;
            var clientConfig = config.Client;

            if (appwriteConfig == null || string.IsNullOrEmpty(appwriteConfig.Endpoint) ||
                string.IsNullOrEmpty(appwriteConfig.ProjectId) || string.IsNullOrEmpty(appwriteConfig.ApiKey))
            {
                ShowError("Cấu hình Appwrite chưa đầy đủ. Vui lòng kiểm tra AppConfig.json");
                return;
            }

            _appwriteService = new AppwriteService(
                appwriteConfig.Endpoint,
                appwriteConfig.ProjectId,
                appwriteConfig.ApiKey
            );

            _versionManagerService = new VersionManagerService(_appwriteService, _configService);
            _versionManagerService.ProgressUpdate += (s, msg) => UpdateStatus(msg);
            _versionManagerService.ErrorOccurred += (s, msg) => ShowError(msg);

            // Register client
            _ = Task.Run(async () =>
            {
                try
                {
                    var client = new Client
                    {
                        ClientId = clientConfig?.ClientId,
                        Product = clientConfig?.Product,
                        Site = clientConfig?.Site,
                        Tenant = clientConfig?.Tenant,
                        CurrentVersion = GetCurrentVersion(),
                        Os = Environment.OSVersion.Platform.ToString(),
                        Arch = Environment.Is64BitOperatingSystem ? "x64" : "x86",
                        UpdateChannel = clientConfig?.UpdateChannel ?? "stable",
                        Status = "active"
                    };
                    await _appwriteService.RegisterClientAsync(client);
                }
                catch (Exception ex)
                {
                    LogMessage($"Lỗi đăng ký client: {ex.Message}");
                }
            });
        }

        private void LoadCurrentVersion()
        {
            var version = GetCurrentVersion();
            var config = _configService.GetConfig();
            var clientConfig = config.Client;

            this.Invoke((MethodInvoker)delegate
            {
                labelCurrentVersionValue.Text = version;
                labelProductValue.Text = clientConfig?.Product ?? "MTVS";
                labelChannelValue.Text = clientConfig?.UpdateChannel ?? "stable";
            });
        }

        private string GetCurrentVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }

        private async void buttonCheckUpdate_Click(object sender, EventArgs e)
        {
            if (_appwriteService == null)
            {
                ShowError("Service chưa được khởi tạo");
                return;
            }

            buttonCheckUpdate.Enabled = false;
            progressBar.Visible = true;
            progressBar.Value = 0;
            UpdateStatus("Đang kiểm tra cập nhật...");

            try
            {
                var config = _configService.GetConfig();
                var clientConfig = config.Client;
                var currentVersion = GetCurrentVersion();
                var os = Environment.OSVersion.Platform.ToString();
                var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";

                _currentUpdateCheck = await _appwriteService.CheckLatestVersionAsync(
                    clientConfig?.Product ?? "MTVS",
                    clientConfig?.UpdateChannel ?? "stable",
                    currentVersion,
                    os,
                    arch
                );

                this.Invoke((MethodInvoker)delegate
                {
                    if (_currentUpdateCheck.HasUpdate)
                    {
                        labelLatestVersionValue.Text = _currentUpdateCheck.LatestRelease?.Version ?? "---";
                        labelLatestVersionValue.ForeColor = Color.Green;
                        labelLatestVersionValue.Symbol = 61533; // Check icon
                        textBoxChangelog.Text = _currentUpdateCheck.Changelog ?? "Không có thông tin thay đổi";
                        buttonUpdate.Enabled = true;
                        UpdateStatus("Có phiên bản mới!");
                    }
                    else
                    {
                        labelLatestVersionValue.Text = "Đã là phiên bản mới nhất";
                        labelLatestVersionValue.ForeColor = Color.Blue;
                        labelLatestVersionValue.Symbol = 61528; // Info icon
                        textBoxChangelog.Text = "";
                        buttonUpdate.Enabled = false;
                        UpdateStatus(_currentUpdateCheck.ErrorMessage ?? "Đã là phiên bản mới nhất");
                    }
                });

                // Report event
                await _appwriteService.ReportEventAsync(
                    clientConfig?.ClientId ?? "",
                    "check",
                    clientConfig?.Product ?? "MTVS",
                    currentVersion,
                    _currentUpdateCheck.HasUpdate ? "Update available" : "No update"
                );
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi kiểm tra cập nhật: {ex.Message}");
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate
                {
                    buttonCheckUpdate.Enabled = true;
                    progressBar.Visible = false;
                });
            }
        }

        private async void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (_appwriteService == null || _versionManagerService == null || _currentUpdateCheck == null || !_currentUpdateCheck.HasUpdate)
            {
                ShowError("Không có cập nhật để cài đặt");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn cập nhật? Hệ thống sẽ tự động sao lưu trước khi cập nhật.",
                "Xác nhận cập nhật",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
            {
                return;
            }

            buttonUpdate.Enabled = false;
            buttonCheckUpdate.Enabled = false;
            progressBar.Visible = true;
            progressBar.Value = 0;

            try
            {
                var release = _currentUpdateCheck.LatestRelease;
                if (release == null || string.IsNullOrEmpty(_currentUpdateCheck.DownloadUrl))
                {
                    throw new Exception("Thông tin release không hợp lệ");
                }

                // Download manifest
                Manifest? manifest = null;
                if (!string.IsNullOrEmpty(_currentUpdateCheck.ManifestUrl))
                {
                    manifest = await _appwriteService.DownloadManifestAsync(_currentUpdateCheck.ManifestUrl);
                }

                if (manifest == null)
                {
                    throw new Exception("Không thể tải manifest");
                }

                // Create backup first
                _currentBackupPath = await _versionManagerService.CreateBackupAsync(manifest);

                // Perform update
                var success = await _versionManagerService.UpdateAsync(
                    release,
                    manifest,
                    _currentUpdateCheck.DownloadUrl
                );

                if (success)
                {
                    MessageBox.Show(
                        "Cập nhật thành công! Ứng dụng sẽ khởi động lại.",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    LoadCurrentVersion();
                    buttonRollback.Enabled = true;
                }
                else
                {
                    MessageBox.Show(
                        "Cập nhật thất bại. Hệ thống đã tự động khôi phục từ bản backup.",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi cập nhật: {ex.Message}");
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate
                {
                    buttonUpdate.Enabled = _currentUpdateCheck?.HasUpdate ?? false;
                    buttonCheckUpdate.Enabled = true;
                    progressBar.Visible = false;
                });
            }
        }

        private async void buttonRollback_Click(object sender, EventArgs e)
        {
            if (_versionManagerService == null || string.IsNullOrEmpty(_currentBackupPath))
            {
                ShowError("Không có backup để khôi phục");
                return;
            }

            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn khôi phục từ backup?",
                "Xác nhận khôi phục",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result != DialogResult.Yes)
            {
                return;
            }

            buttonRollback.Enabled = false;
            progressBar.Visible = true;

            try
            {
                var success = await _versionManagerService.RollbackAsync(_currentBackupPath);
                if (success)
                {
                    MessageBox.Show(
                        "Khôi phục thành công!",
                        "Thành công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    LoadCurrentVersion();
                }
                else
                {
                    MessageBox.Show(
                        "Khôi phục thất bại.",
                        "Lỗi",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                ShowError($"Lỗi khôi phục: {ex.Message}");
            }
            finally
            {
                this.Invoke((MethodInvoker)delegate
                {
                    buttonRollback.Enabled = !string.IsNullOrEmpty(_currentBackupPath);
                    progressBar.Visible = false;
                });
            }
        }

        private void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { UpdateStatus(message); });
                return;
            }

            labelStatus.Text = message;
            labelStatus.Symbol = 61528; // Info icon
            labelStatus.ForeColor = Color.Black;
            LogMessage(message);
        }

        private void ShowError(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { ShowError(message); });
                return;
            }

            labelStatus.Text = $"Lỗi: {message}";
            labelStatus.Symbol = 61527; // Error icon
            labelStatus.ForeColor = Color.Red;
            LogMessage($"ERROR: {message}");
        }

        private void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { LogMessage(message); });
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            textBoxLog.Text += $"[{timestamp}] {message}\r\n";
            textBoxLog.SelectionStart = textBoxLog.Text.Length;
            textBoxLog.ScrollToCaret();
        }
    }
}
