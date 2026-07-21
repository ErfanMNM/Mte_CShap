using Sunny.UI;
using CProject.Module;
using System.ComponentModel;
using Microsoft.Win32;

namespace CProject.Views
{
    public partial class FDashboard : UIPage
    {
        private readonly DataPoolModule _dataPool = new DataPoolModule();
        private BackgroundWorker? _workerImportFile;
        private string _workerFilePath = string.Empty;
        private string _workerPoolName = string.Empty;
        private int _currentPageA4 = 1;
        private int _currentPageA7 = 1;
        private PoolCodePageResult? _lastCodesResultA4;
        private PoolListResult? _lastPoolsResultA7;

        public FDashboard()
        {
            InitializeComponent();
            InitBackgroundWorker();
        }

        private void InitBackgroundWorker()
        {
            _workerImportFile = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            _workerImportFile.DoWork += WorkerImportFile_DoWork;
            _workerImportFile.ProgressChanged += WorkerImportFile_ProgressChanged;
            _workerImportFile.RunWorkerCompleted += WorkerImportFile_RunWorkerCompleted;
        }

        private void WorkerImportFile_DoWork(object? sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var args = e.Argument as object[];
            string poolName = (string)args![0];
            string filePath = (string)args[1];

            var result = _dataPool.AddCodes(
                poolName: poolName,
                mode: 0,
                filePath: filePath,
                singleCode: null,
                dataTable: null,
                createID: "SYSTEM",
                createdBy: "Admin",
                progressCallback: (current, total) =>
                {
                    int percent = (int)((double)current / total * 100);
                    worker?.ReportProgress(percent, $"Đang xử lý: {current}/{total}");
                }
            );

            e.Result = result;
        }

        private void WorkerImportFile_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            AddLog(e.UserState?.ToString() ?? "");
        }

        private void WorkerImportFile_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            btnAddCodeFile.Enabled = true;
            btnCreatePool.Enabled = true;
            btnAddCodeSingle.Enabled = true;
            btnGetPoolInfo.Enabled = true;

            if (e.Error != null)
            {
                AddLog($"Lỗi: {e.Error.Message}");
            }
            else if (e.Cancelled)
            {
                AddLog("Đã hủy!");
            }
            else
            {
                var result = e.Result as DataPoolAddCodesResult;
                if (result != null)
                {
                    AddLog($"Thêm từ File: {result.Message}");
                }
            }
        }

        private void AddLog(string message)
        {
            if (lstResult.InvokeRequired)
            {
                lstResult.Invoke(new Action(() =>
                {
                    lstResult.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                    lstResult.SelectedIndex = lstResult.Items.Count - 1;
                }));
            }
            else
            {
                lstResult.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                lstResult.SelectedIndex = lstResult.Items.Count - 1;
            }
        }

        private void btnCreatePool_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolName.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập tên Pool!");
                return;
            }

            var poolInfo = new DataPoolModule.PoolInfo(
                id: 0,
                name: poolName,
                description: "Test Pool",
                batchID: "BATCH001",
                createID: "SYSTEM",
                note: "Created for testing",
                createdBy: "Admin",
                createDatetime: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            );

            var result = _dataPool.CreatePool(poolInfo);
            if (result.Success)
            {
                AddLog($"Tạo Pool thành công: {poolName}");
                AddLog($"Đường dẫn: {result.Data}");
            }
            else
            {
                AddLog($"Lỗi: {result.Message}");
            }
        }

        private void btnAddCodeSingle_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolName.Text.Trim();
            string code = txtCode.Text.Trim();

            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập tên Pool!");
                return;
            }
            if (string.IsNullOrEmpty(code))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Code!");
                return;
            }

            var result = _dataPool.AddCodes(
                poolName: poolName,
                mode: 1,
                filePath: null,
                singleCode: code,
                dataTable: null,
                createID: "SYSTEM",
                createdBy: "Admin"
            );

            AddLog($"Thêm 1 Code: {result.Message}");
        }

        private void btnAddCodeFile_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolName.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập tên Pool!");
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files|*.txt;*.csv|All Files|*.*",
                Title = "Chọn file chứa mã code"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Disable buttons during import
                btnAddCodeFile.Enabled = false;
                btnCreatePool.Enabled = false;
                btnAddCodeSingle.Enabled = false;
                btnGetPoolInfo.Enabled = false;

                AddLog($"Bắt đầu import file: {openFileDialog.SafeFileName}");
                
                _workerImportFile!.RunWorkerAsync(new object[] { poolName, openFileDialog.FileName });
            }
        }

        private void btnGetPoolInfo_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolName.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập tên Pool!");
                return;
            }

            var pathResult = _dataPool.GetPoolPath(poolName);
            if (pathResult.Success)
            {
                AddLog($"Pool Path: {pathResult.Data}");
                if (System.IO.File.Exists(pathResult.Data))
                {
                    AddLog("Pool tồn tại!");
                }
                else
                {
                    AddLog("Pool không tồn tại!");
                }
            }
            else
            {
                AddLog($"Lỗi: {pathResult.Message}");
            }
        }

        private void btnA1UpdateStatus_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolNameA1.Text.Trim();
            string poolCode = txtPoolCodeA1.Text.Trim();
            string codeIdText = txtCodeIDA1.Text.Trim();
            string newStatusText = txtNewStatus.Text.Trim();

            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Pool Name!");
                return;
            }
            if (string.IsNullOrEmpty(poolCode) && string.IsNullOrEmpty(codeIdText))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập PoolCode hoặc Code ID!");
                return;
            }
            if (!int.TryParse(newStatusText, out int newStatus))
            {
                UIMessageBox.ShowWarning("NewStatus phải là số nguyên (0, 1, -1)!");
                return;
            }

            double? id = string.IsNullOrEmpty(codeIdText) ? null : double.Parse(codeIdText);

            var result = _dataPool.UpdateCodeStatus(poolName, poolCode, id, newStatus);
            AddLog($"[A1] UpdateStatus: {result.Message}");
        }

        private void btnA2GetPoolInfo_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolNameA1.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Pool Name!");
                return;
            }

            var result = _dataPool.GetPoolInfo(poolName);
            AddLog($"[A2] GetPoolInfo: {result.Message}");

            if (result.Success && result.Data != null)
            {
                dgvResultA1A5.DataSource = null;
                dgvResultA1A5.Columns.Clear();
                dgvResultA1A5.Rows.Clear();
                dgvResultA1A5.Columns.Add("Property", "Property");
                dgvResultA1A5.Columns.Add("Value", "Value");
                dgvResultA1A5.Columns[0].Width = 200;
                dgvResultA1A5.Columns[1].Width = 300;

                dgvResultA1A5.Rows.Add("ID", result.Data.ID);
                dgvResultA1A5.Rows.Add("PoolName", result.Data.PoolName);
                dgvResultA1A5.Rows.Add("PoolDescription", result.Data.PoolDescription);
                dgvResultA1A5.Rows.Add("PoolCreatedBy", result.Data.PoolCreatedBy);
                dgvResultA1A5.Rows.Add("PoolCreateDatetime", result.Data.PoolCreateDatetime);

                if (result.Data.Count != null)
                {
                    dgvResultA1A5.Rows.Add("---Count---", "---");
                    dgvResultA1A5.Rows.Add("TotalCount", result.Data.Count.TotalCount);
                    dgvResultA1A5.Rows.Add("UnusedCount", result.Data.Count.UnusedCount);
                    dgvResultA1A5.Rows.Add("UsedCount", result.Data.Count.UsedCount);
                    dgvResultA1A5.Rows.Add("ErrorCount", result.Data.Count.ErrorCount);
                }

                txtPoolNameA6.Text = poolName;
            }
        }

        private void btnA3GetPoolCode_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolNameA1.Text.Trim();
            string poolCode = txtPoolCodeA1.Text.Trim();
            string codeIdText = txtCodeIDA1.Text.Trim();

            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Pool Name!");
                return;
            }
            if (string.IsNullOrEmpty(poolCode) && string.IsNullOrEmpty(codeIdText))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập PoolCode hoặc Code ID!");
                return;
            }

            double? id = string.IsNullOrEmpty(codeIdText) ? null : double.Parse(codeIdText);
            var result = _dataPool.GetPoolCode(poolName, poolCode, id);
            AddLog($"[A3] GetPoolCode: {result.Message}");

            if (result.Success && result.Data != null)
            {
                dgvResultA1A5.DataSource = result.Data;
            }
        }

        private void btnA4GetCodesPaginated_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolNameA1.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Pool Name!");
                return;
            }

            _currentPageA4 = 1;
            LoadCodesPaginated(poolName, _currentPageA4);
        }

        private void LoadCodesPaginated(string poolName, int pageIndex)
        {
            int? status = null;
            if (cboFilterStatus.SelectedIndex > 0)
            {
                string[] parts = cboFilterStatus.Text.Split('-');
                if (parts.Length > 0 && int.TryParse(parts[0], out int s))
                    status = s;
            }

            string? batchID = string.IsNullOrWhiteSpace(txtFilterBatchID.Text) ? null : txtFilterBatchID.Text.Trim();

            var result = _dataPool.GetPoolCodesPaginated(poolName, pageIndex, 100, status, batchID);

            AddLog($"[A4] Page {pageIndex}: {result.Message}");

            if (result.Success && result.Data != null)
            {
                _lastCodesResultA4 = result.Data;
                dgvResultA1A5.DataSource = result.Data.Data;
                lblPageInfoA4.Text = $"Page: {result.Data.PageIndex} / {result.Data.TotalPages} (Total: {result.Data.TotalCount})";
            }
            else
            {
                _lastCodesResultA4 = null;
                dgvResultA1A5.DataSource = null;
                lblPageInfoA4.Text = "No data";
            }
        }

        private void btnPrevPage_Click(object? sender, EventArgs e)
        {
            if (_lastCodesResultA4 == null || _lastCodesResultA4.PageIndex <= 1) return;
            string poolName = txtPoolNameA1.Text.Trim();
            if (string.IsNullOrEmpty(poolName)) return;

            _currentPageA4 = _lastCodesResultA4.PageIndex - 1;
            LoadCodesPaginated(poolName, _currentPageA4);
        }

        private void btnNextPage_Click(object? sender, EventArgs e)
        {
            if (_lastCodesResultA4 == null || !_lastCodesResultA4.HasNextPage) return;
            string poolName = txtPoolNameA1.Text.Trim();
            if (string.IsNullOrEmpty(poolName)) return;

            _currentPageA4 = _lastCodesResultA4.PageIndex + 1;
            LoadCodesPaginated(poolName, _currentPageA4);
        }

        private void btnA5GetCounts_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolNameA1.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Pool Name!");
                return;
            }

            var result = _dataPool.GetCodeCounts(poolName);
            AddLog($"[A5] GetCodeCounts: {result.Message}");

            if (result.Success && result.Data != null)
            {
                dgvResultA1A5.DataSource = null;
                dgvResultA1A5.Columns.Clear();
                dgvResultA1A5.Rows.Clear();
                dgvResultA1A5.Columns.Add("Metric", "Metric");
                dgvResultA1A5.Columns.Add("Count", "Count");
                dgvResultA1A5.Columns[0].Width = 200;
                dgvResultA1A5.Columns[1].Width = 100;

                dgvResultA1A5.Rows.Add("TotalCount", result.Data.TotalCount);
                dgvResultA1A5.Rows.Add("UsedCount", result.Data.UsedCount);
            }
        }

        private void btnA6GetCodesByStatus_Click(object? sender, EventArgs e)
        {
            string poolName = txtPoolNameA6.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập Pool Name!");
                return;
            }

            int? status = null;
            if (cboStatusFilterA6.SelectedIndex > 0)
            {
                string[] parts = cboStatusFilterA6.Text.Split('-');
                if (parts.Length > 0 && int.TryParse(parts[0], out int s))
                    status = s;
            }

            var result = _dataPool.GetCodesByStatus(poolName, status);
            AddLog($"[A6] GetCodesByStatus: {result.Message}");

            if (result.Success && result.Data != null)
            {
                dgvResultA6A7.DataSource = result.Data;
                lblPageInfoA7.Text = $"Total: {result.Data.Rows.Count}";
            }
            else
            {
                dgvResultA6A7.DataSource = null;
            }
        }

        private void btnA7GetPoolsPaginated_Click(object? sender, EventArgs e)
        {
            _currentPageA7 = 1;
            LoadPoolsPaginated(_currentPageA7);
        }

        private void LoadPoolsPaginated(int pageIndex)
        {
            var result = _dataPool.GetPoolsPaginated(pageIndex);
            AddLog($"[A7] Page {pageIndex}: {result.Message}");

            if (result.Success && result.Data != null)
            {
                _lastPoolsResultA7 = result.Data;
                dgvResultA6A7.DataSource = result.Data.Items;
                lblPageInfoA7.Text = $"Page: {result.Data.PageIndex} / {result.Data.TotalPages} (Total: {result.Data.TotalCount})";
            }
            else
            {
                _lastPoolsResultA7 = null;
                dgvResultA6A7.DataSource = null;
                lblPageInfoA7.Text = "No data";
            }
        }

        private void btnPrevPageA7_Click(object? sender, EventArgs e)
        {
            if (_lastPoolsResultA7 == null || _lastPoolsResultA7.PageIndex <= 1) return;

            _currentPageA7 = _lastPoolsResultA7.PageIndex - 1;
            LoadPoolsPaginated(_currentPageA7);
        }

        private void btnNextPageA7_Click(object? sender, EventArgs e)
        {
            if (_lastPoolsResultA7 == null || !_lastPoolsResultA7.HasNextPage) return;

            _currentPageA7 = _lastPoolsResultA7.PageIndex + 1;
            LoadPoolsPaginated(_currentPageA7);
        }
    }
}
