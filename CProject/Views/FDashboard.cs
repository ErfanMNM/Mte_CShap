using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Sunny.UI;
using CProject.Module;

namespace CProject.Views
{
    public partial class FDashboard : UIPage
    {
        private readonly DataPoolModule _dataPool = new DataPoolModule();

        public FDashboard()
        {
            InitializeComponent();
            AddLog("Sẵn sàng. Thiết lập Pool Name ở panel trên cùng rồi bấm CreatePool.");
        }

        // ===== Helpers ========================================================

        private string GetPoolNameOrWarn()
        {
            string poolName = txtPoolNameSession.Text.Trim();
            if (string.IsNullOrEmpty(poolName))
            {
                UIMessageBox.ShowWarning("Vui lòng nhật Pool Name ở panel 'Thiết lập phiên test'.");
                AddLog("Cảnh báo: PoolName trống.");
            }
            return poolName;
        }

        private bool TryParseStatus(int? defaultValue, out int? status)
        {
            status = defaultValue;
            return true;
        }

        private int? GetSelectedStatus(Sunny.UI.UIComboBox cbo)
        {
            switch (cbo.SelectedIndex)
            {
                case 1: return 0;
                case 2: return 1;
                case 3: return -1;
                default: return null;
            }
        }

        private string BuildDataTableSummary(DataTable? dt, int maxRows = 5)
        {
            if (dt == null) return "(null)";
            if (dt.Rows.Count == 0) return "(0 dòng)";
            var sb = new StringBuilder();
            int show = Math.Min(dt.Rows.Count, maxRows);
            sb.AppendLine($"Bảng {dt.TableName ?? ""} : {dt.Rows.Count} dòng, {dt.Columns.Count} cột");
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                sb.Append("[").Append(dt.Columns[i].ColumnName).Append("] ");
            }
            sb.AppendLine();
            for (int i = 0; i < show; i++)
            {
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    sb.Append(dt.Rows[i][c]?.ToString() ?? "null");
                    sb.Append(" | ");
                }
                sb.AppendLine();
            }
            if (dt.Rows.Count > show)
            {
                sb.AppendLine($"... ({dt.Rows.Count - show} dòng còn lại)");
            }
            return sb.ToString();
        }

        private T Run<T>(string apiName, Func<T> action)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                T result = action();
                sw.Stop();
                AddLog($"[{apiName}] hoàn tất trong {sw.ElapsedMilliseconds} ms.");
                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                AddLog($"[{apiName}] LỖI sau {sw.ElapsedMilliseconds} ms: {ex.Message}");
                UIMessageBox.ShowError($"{apiName}\n\n{ex.Message}");
                return default!;
            }
        }

        private void AddLog(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";
            if (lstLog.InvokeRequired)
            {
                lstLog.BeginInvoke(new Action(() => lstLog.Items.Add(line)));
            }
            else
            {
                lstLog.Items.Add(line);
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }
        }

        // ===== Session ========================================================

        private void btnResetSession_Click(object? sender, EventArgs e)
        {
            txtPoolNameSession.Text = "TestPool";
            txtCreateID.Text = "TESTER";
            txtCreatedBy.Text = "Admin";
            txtSingleCode.Text = "CODE001";
            txtQueryPoolCode.Text = string.Empty;
            txtQueryCodeID.Text = string.Empty;
            txtFilterBatchID.Text = string.Empty;
            txtUpdatePoolCode.Text = string.Empty;
            txtUpdateCodeID.Text = string.Empty;
            cboUpdateStatus.SelectedIndex = 0;
            cboFilterStatus.SelectedIndex = 0;
            cboStatusOnly.SelectedIndex = 0;
            txtFilePath.Text = "(chưa chọn)";
            AddLog("Đã reset phiên test về giá trị mặc định.");
        }

        private void btnLoadSamples_Click(object? sender, EventArgs e)
        {
            txtPoolNameSession.Text = "TestPool";
            txtCreateID.Text = "TESTER";
            txtCreatedBy.Text = "Admin";
            txtSingleCode.Text = "SAMPLE-CODE-001";
            AddLog("Đã load dữ liệu mẫu cho Pool/CreatedBy/SingleCode.");
        }

        private void btnClearLog_Click(object? sender, EventArgs e)
        {
            lstLog.Items.Clear();
            AddLog("Đã xóa log.");
        }

        private void btnCopyLog_Click(object? sender, EventArgs e)
        {
            try
            {
                if (lstLog.Items.Count == 0) return;
                var sb = new StringBuilder();
                foreach (var item in lstLog.Items)
                {
                    sb.AppendLine(item?.ToString());
                }
                Clipboard.SetText(sb.ToString());
                AddLog($"Đã copy {lstLog.Items.Count} dòng log vào clipboard.");
            }
            catch (Exception ex)
            {
                UIMessageBox.ShowWarning($"Không copy được: {ex.Message}");
            }
        }

        // ===== Lifecycle tab ==================================================

        private void btnGetPoolPath_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            var result = Run("GetPoolPath", () => _dataPool.GetPoolPath(poolName));
            if (result == null) return;
            AddLog($"GetPoolPath: Success={result.Success}, Path='{result.Data}'");
            if (result.Success)
            {
                UIMessageBox.ShowInfo($"Đường dẫn:\n{result.Data}");
            }
        }

        private void btnCreatePool_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            string createdBy = txtCreatedBy.Text.Trim();
            string createID = txtCreateID.Text.Trim();
            if (string.IsNullOrEmpty(createdBy)) createdBy = "Admin";
            if (string.IsNullOrEmpty(createID)) createID = "TESTER";

            var info = new DataPoolModule.PoolInfo(
                id: 0,
                name: poolName,
                description: $"Auto-created by FDashboard on {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                batchID: string.Empty,
                createID: createID,
                note: "Tạo bằng giao diện kiểm thử FDashboard.",
                createdBy: createdBy,
                createDatetime: DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            );
            var result = Run("CreatePool", () => _dataPool.CreatePool(info));
            if (result == null) return;
            AddLog($"CreatePool: Success={result.Success}, Path='{result.Data}'");
            if (result.Success)
            {
                UIMessageBox.ShowInfo($"Đã tạo pool:\n{result.Data}");
            }
        }

        private void btnGetPoolInfo_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            var result = Run("GetPoolInfo", () => _dataPool.GetPoolInfo(poolName));
            if (result == null || result.Data == null) return;
            var info = result.Data;
            var sb = new StringBuilder();
            sb.AppendLine($"ID={info.ID}, Name={info.PoolName}");
            sb.AppendLine($"Description={info.PoolDescription}");
            sb.AppendLine($"CreateID={info.PoolCreateID}, CreatedBy={info.PoolCreatedBy}");
            sb.AppendLine($"CreateDatetime={info.PoolCreateDatetime}");
            sb.AppendLine($"Note={info.PoolNote}");
            if (info.Count != null)
            {
                sb.AppendLine($"Counts: Total={info.Count.TotalCount}, Unused={info.Count.UnusedCount}, Used={info.Count.UsedCount}, Error={info.Count.ErrorCount}");
            }
            AddLog(sb.ToString());
            UIMessageBox.ShowInfo(sb.ToString());
        }

        private void btnGetPoolsPaginated_Click(object? sender, EventArgs e)
        {
            var result = Run("GetPoolsPaginated", () => _dataPool.GetPoolsPaginated(1, 100));
            if (result == null) return;
            AddLog($"GetPoolsPaginated: Success={result.Success}, Message='{result.Message}'");
            if (!result.Success)
            {
                UIMessageBox.ShowWarning(result.Message);
                return;
            }
            if (result.Data == null || result.Data.Items.Count == 0)
            {
                AddLog("Không có Pool nào trong thư mục databasePath.");
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine($"Tổng: {result.Data.TotalCount}, Trang: {result.Data.PageIndex}/{result.Data.TotalPages}");
            foreach (var p in result.Data.Items)
            {
                sb.AppendLine($"#{p.ID} | {p.PoolName} | by {p.PoolCreatedBy} | {p.PoolCreateDatetime}");
            }
            AddLog(sb.ToString());
            UIMessageBox.ShowInfo(sb.ToString());
        }

        // ===== Write tab ======================================================

        private void btnBrowseFile_Click(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Chọn file chứa các code (mỗi dòng 1 code)",
                Filter = "Text/CSV files (*.txt;*.csv)|*.txt;*.csv|All files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = dlg.FileName;
                AddLog($"Đã chọn file: {dlg.FileName}");
            }
        }

        private void btnAddCodeSingle_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            string code = txtSingleCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập 1 code ở ô bên cạnh.");
                return;
            }
            string createID = txtCreateID.Text.Trim();
            string createdBy = txtCreatedBy.Text.Trim();
            if (string.IsNullOrEmpty(createID)) createID = "TESTER";
            if (string.IsNullOrEmpty(createdBy)) createdBy = "Admin";

            var result = Run("AddCodes(single)", () => _dataPool.AddCodes(poolName, 1, null, code, null, createID, createdBy));
            if (result == null) return;
            AddLog($"AddCodes(single): Success={result.Success}, Message='{result.Message}'");
        }

        private void btnAddCodeFile_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            string filePath = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(filePath) || filePath == "(chưa chọn)" || !File.Exists(filePath))
            {
                UIMessageBox.ShowWarning("Vui lòng chọn file hợp lệ trước khi thêm.");
                return;
            }
            string createID = txtCreateID.Text.Trim();
            string createdBy = txtCreatedBy.Text.Trim();
            if (string.IsNullOrEmpty(createID)) createID = "TESTER";
            if (string.IsNullOrEmpty(createdBy)) createdBy = "Admin";

            var result = Run("AddCodes(file)", () => _dataPool.AddCodes(poolName, 0, filePath, null, null, createID, createdBy));
            if (result == null) return;
            AddLog($"AddCodes(file): Success={result.Success}, Added={result.AddedCount}, Duplicate={result.DuplicateCount}, Error={result.ErrorCount}, Message='{result.Message}'");
        }

        private void btnUpdateStatus_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            string poolCode = txtUpdatePoolCode.Text.Trim();
            string idText = txtUpdateCodeID.Text.Trim();
            if (string.IsNullOrEmpty(poolCode) && string.IsNullOrEmpty(idText))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập PoolCode hoặc CodeID.");
                return;
            }
            if (cboUpdateStatus.SelectedIndex <= 0)
            {
                UIMessageBox.ShowWarning("Vui lòng chọn status mới.");
                return;
            }
            int newStatus = cboUpdateStatus.SelectedIndex switch
            {
                1 => 0,
                2 => 1,
                3 => -1,
                _ => 0
            };
            double? id = null;
            if (!string.IsNullOrEmpty(idText))
            {
                if (!double.TryParse(idText, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    UIMessageBox.ShowWarning("CodeID phải là số.");
                    return;
                }
                id = parsed;
            }
            var result = Run("UpdateCodeStatus", () => _dataPool.UpdateCodeStatus(poolName, poolCode, id, newStatus));
            if (result == null) return;
            AddLog($"UpdateCodeStatus: Success={result.Success}, Message='{result.Message}'");
        }

        // ===== Read tab =======================================================

        private void btnGetPoolCode_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            string poolCode = txtQueryPoolCode.Text.Trim();
            string idText = txtQueryCodeID.Text.Trim();
            if (string.IsNullOrEmpty(poolCode) && string.IsNullOrEmpty(idText))
            {
                UIMessageBox.ShowWarning("Vui lòng nhập PoolCode hoặc CodeID.");
                return;
            }
            double? id = null;
            if (!string.IsNullOrEmpty(idText))
            {
                if (!double.TryParse(idText, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    UIMessageBox.ShowWarning("CodeID phải là số.");
                    return;
                }
                id = parsed;
            }
            var result = Run("GetPoolCode", () => _dataPool.GetPoolCode(poolName, poolCode, id));
            if (result == null) return;
            AddLog($"GetPoolCode: Success={result.Success}, Message='{result.Message}'");
            AddLog(BuildDataTableSummary(result.Data));
        }

        private void btnGetCodesPaginated_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            int? status = GetSelectedStatus(cboFilterStatus);
            string batchID = txtFilterBatchID.Text.Trim();
            var result = Run("GetCodesPaginated", () => _dataPool.GetPoolCodesPaginated(poolName, 1, 100, status, string.IsNullOrEmpty(batchID) ? null : batchID));
            if (result == null) return;
            AddLog($"GetCodesPaginated: Success={result.Success}, Total={result.Data?.TotalCount ?? 0}, Page={result.Data?.PageIndex}/{result.Data?.TotalPages}");
            AddLog(BuildDataTableSummary(result.Data?.Data));
        }

        private void btnGetCodesByStatus_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            int? status = GetSelectedStatus(cboStatusOnly);
            var result = Run("GetCodesByStatus", () => _dataPool.GetCodesByStatus(poolName, status));
            if (result == null) return;
            AddLog($"GetCodesByStatus: Success={result.Success}, Message='{result.Message}'");
            AddLog(BuildDataTableSummary(result.Data));
        }

        private void btnGetCodeCounts_Click(object? sender, EventArgs e)
        {
            string poolName = GetPoolNameOrWarn();
            if (string.IsNullOrEmpty(poolName)) return;
            var result = Run("GetCodeCounts", () => _dataPool.GetCodeCounts(poolName));
            if (result == null || result.Data == null) return;
            var c = result.Data;
            AddLog($"GetCodeCounts: Total={c.TotalCount}, Used={c.UsedCount}, Message='{result.Message}'");
            UIMessageBox.ShowInfo($"Tổng: {c.TotalCount}\nĐã dùng: {c.UsedCount}\nCòn lại: {c.TotalCount - c.UsedCount}");
        }
    }
}
