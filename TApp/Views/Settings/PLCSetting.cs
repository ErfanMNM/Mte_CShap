using HslCommunication;
using HslCommunication.Profinet.Omron;
using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Utils;
using TTManager.Diaglogs;

namespace TApp.Views.Settings
{
    

    public partial class PLCSetting : UIPage
    {
        #region Fields & Properties
        private RecipeManager _recipeManager;
        private BackgroundWorker _bgwSavePLC;
        private BackgroundWorker _bgwUpdatePLCValues;
        private BackgroundWorker _bgwCustomRead;
        private BackgroundWorker _bgwCustomWrite;

        public bool IsPageOpen { get; set; } = false;
        private bool _isLoading = false;

        public static PLC_Parameter PLC_Parameter_On_PLC { get; set; } = new PLC_Parameter();
        #endregion

        #region Constructor & Page Initialization
        public PLCSetting()
        {
            InitializeComponent();
            InitializeBackgroundWorkers();
        }

        public void INIT()
        {
            _isLoading = true;

            var defaultConfig = new PLC_Parameter { DelayCamera = "1000", DelayReject = "2000", RejectStreng = "20" };
            _recipeManager = new RecipeManager("PLC_RECIPEs", defaultConfig);
            _recipeManager.Initialize();

            omronplC_Hsl1.PLC_IP = AppConfigs.Current.PLC_Test_Mode ? "127.0.0.1" : AppConfigs.Current.PLC_IP;
            omronplC_Hsl1.PLC_PORT = AppConfigs.Current.PLC_Test_Mode ? 9600 : AppConfigs.Current.PLC_Port;
            omronplC_Hsl1.InitPLC();

            UpdateUIFromRecipe(_recipeManager.CurrentRecipeOnPC);
            LoadRecipeList();

            if (!_bgwUpdatePLCValues.IsBusy)
            {
                _bgwUpdatePLCValues.RunWorkerAsync();
            }

            _isLoading = false;
        }

        private void InitializeBackgroundWorkers()
        {
            _bgwSavePLC = new BackgroundWorker();
            _bgwSavePLC.DoWork += bgwSavePLC_DoWork;
            _bgwSavePLC.RunWorkerCompleted += bgwSavePLC_RunWorkerCompleted;

            _bgwUpdatePLCValues = new BackgroundWorker { WorkerSupportsCancellation = true };
            _bgwUpdatePLCValues.DoWork += bgwUpdatePLCValues_DoWork;

            _bgwCustomRead = new BackgroundWorker();
            _bgwCustomRead.DoWork += bgwCustomRead_DoWork;

            _bgwCustomWrite = new BackgroundWorker();
            _bgwCustomWrite.DoWork += bgwCustomWrite_DoWork;
        }

        private void PLCSetting_Initialize(object sender, EventArgs e)
        {
            IsPageOpen = true;
            webView21.Source = new Uri($"http://{AppConfigs.Current.Camera_01_IP}/monitor");
            GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Mở trang cài đặt PLC", "", "UA-PLCSETTING-01");
        }

        private void PLCSetting_Finalize(object sender, EventArgs e)
        {
            IsPageOpen = false;
            webView21.Source = new Uri("about:blank");
        }

        private void LoadRecipeList()
        {
            ipRecipe.Items.Clear();
            string[] files = Directory.GetFiles("PLC_RECIPEs", "*.rplc");
            foreach (var file in files)
            {
                ipRecipe.Items.Add(Path.GetFileNameWithoutExtension(file));
            }
            ipRecipe.SelectedItem = _recipeManager.SelectedRecipeName;
        }
        #endregion

        #region Recipe Management UI
        private void ipRecipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading || ipRecipe.SelectedItem == null) return;

            string selectedName = ipRecipe.SelectedItem.ToString();
            _recipeManager.SelectRecipe(selectedName);
            UpdateUIFromRecipe(_recipeManager.CurrentRecipeOnPC);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_bgwSavePLC.IsBusy) return;

            var newConfig = new PLC_Parameter
            {
                DelayCamera = ipDelayTriger.Text,
                DelayReject = ipDelayReject.Text,
                RejectStreng = ipRejectStreng.Text
            };
            _bgwSavePLC.RunWorkerAsync(newConfig);
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy giá trị hiện tại từ PLC và hiển thị lên UI
                GetParameterFromPLC();

                var plcParams = new PLC_Parameter
                {
                    DelayCamera = PLC_Parameter_On_PLC.DelayCamera,
                    DelayReject = PLC_Parameter_On_PLC.DelayReject,
                    RejectStreng = PLC_Parameter_On_PLC.RejectStreng
                };

                UpdateUIFromRecipe(plcParams);
                this.ShowSuccessDialog("Đã tải lại giá trị từ PLC thành công!");
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khi tải lại giá trị từ PLC: {ex.Message}");
            }
        }

        private void btnNewRecipe_Click(object sender, EventArgs e)
        {
            using (var enterText = new Entertext { TileText = "Nhập tên Recipe", TextValue = "New Recipe" })
            {
                if (enterText.ShowDialog() == DialogResult.OK)
                {
                    string newName = enterText.TextValue;
                    var currentConfig = new PLC_Parameter { DelayCamera = ipDelayTriger.Text, DelayReject = ipDelayReject.Text, RejectStreng = ipRejectStreng.Text };
                    string errorMessage = _recipeManager.CreateNewRecipe(newName, currentConfig);

                    if (errorMessage != null)
                    {
                        this.ShowErrorDialog(errorMessage);
                    }
                    else
                    {
                        LoadRecipeList();
                        this.ShowSuccessDialog($"Đã tạo Recipe mới: {newName}");
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string selectedRecipe = ipRecipe.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedRecipe)) return;

            if (_recipeManager.DeleteRecipe(selectedRecipe))
            {
                LoadRecipeList();
                UpdateUIFromRecipe(_recipeManager.CurrentRecipeOnPC);
                this.ShowSuccessDialog($"Đã xóa Recipe: {selectedRecipe}");
            }
            else
            {
                this.ShowErrorDialog("Không thể xóa Recipe mặc định.");
            }
        }

        private void UpdateUIFromRecipe(PLC_Parameter recipe)
        {
            ipDelayTriger.Text = recipe.DelayCamera;
            ipDelayReject.Text = recipe.DelayReject;
            ipRejectStreng.Text = recipe.RejectStreng;
        }
        #endregion

        #region PLC Communication
        private void GetParameterFromPLC()
        {
            OperateResult<int[]> read = omronplC_Hsl1.plc.ReadInt32(PLCAddressWithGoogleSheetHelper.Get("PLC_Delay_Camera_DM_C2"), 3);
            if (read.IsSuccess)
            {
                PLC_Parameter_On_PLC = new PLC_Parameter
                {
                    DelayCamera = read.Content[0].ToString(),
                    DelayReject = read.Content[1].ToString(),
                    RejectStreng = read.Content[2].ToString()
                };
            }
            else
            {
                PLC_Parameter_On_PLC = new PLC_Parameter { DelayCamera = "-1", DelayReject = "-1", RejectStreng = "-1" };
            }
        }

        private void bgwUpdatePLCValues_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!_bgwUpdatePLCValues.CancellationPending)
            {
                if (IsPageOpen)
                {
                    GetParameterFromPLC();
                    this.InvokeIfRequired(() =>
                    {
                        opDelayTriger.Text = PLC_Parameter_On_PLC.DelayCamera;
                        opDelayReject.Text = PLC_Parameter_On_PLC.DelayReject;
                        opRejectStreng.Text = PLC_Parameter_On_PLC.RejectStreng;
                    });
                }
                Thread.Sleep(500);
            }
        }

        private void bgwSavePLC_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var plcParams = e.Argument as PLC_Parameter;
                _recipeManager.SaveRecipe(plcParams);
                var values = new int[] { int.Parse(plcParams.DelayCamera), int.Parse(plcParams.DelayReject), int.Parse(plcParams.RejectStreng) };
                OperateResult writeResult = omronplC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Delay_Camera_DM_C2"), values);
                e.Result = new Tuple<OperateResult, PLC_Parameter>(writeResult, plcParams);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void bgwSavePLC_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khi cập nhật: {ex.Message}");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi cập nhật PLC", ex.Message, "ERR-PLCSETTING-02");
                return;
            }

            var resultTuple = (Tuple<OperateResult, PLC_Parameter>)e.Result;
            OperateResult operateResult = resultTuple.Item1;
            PLC_Parameter savedParams = resultTuple.Item2;

            if (operateResult.IsSuccess)
            {
                this.ShowSuccessDialog("Cập nhật thành công!");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.UserAction, "Lưu cài đặt PLC thành công",
                    $"{{'DelayCamera':'{savedParams.DelayCamera}','DelayReject':'{savedParams.DelayReject}','RejectStreng':'{savedParams.RejectStreng}'}}", "UA-PLCSETTING-02");
            }
            else
            {
                this.ShowErrorDialog($"Lỗi khi ghi vào PLC: {operateResult.Message}");
                GlobalVarialbles.Logger?.WriteLogAsync(GlobalVarialbles.CurrentUser.Username, e_LogType.Error, "Lỗi lưu cài đặt PLC", operateResult.Message, "ERR-PLCSETTING-01");
            }
        }
        #endregion

        #region Custom PLC R/W
        private void uiSymbolButton1_Click(object sender, EventArgs e) // Read
        {
            if (_bgwCustomRead.IsBusy)
            {
                this.ShowWarningTip("Đang đọc dữ liệu, vui lòng chờ!");
                return;
            }
            _bgwCustomRead.RunWorkerAsync();
        }

        private void uiSymbolButton2_Click(object sender, EventArgs e) // Write
        {
            if (_bgwCustomWrite.IsBusy)
            {
                this.ShowWarningTip("Đang ghi dữ liệu, vui lòng chờ!");
                return;
            }
            _bgwCustomWrite.RunWorkerAsync();
        }

        private OmronFinsUdp CreateCustomPlcClient()
        {
            return new OmronFinsUdp
            {
                CommunicationPipe = new HslCommunication.Core.Pipe.PipeUdpNet(ipCPLCIP.Text, ipCPLPort.Text.ToInt()) { ReceiveTimeOut = 5000 },
                PlcType = HslCommunication.Profinet.Omron.OmronPlcType.CSCJ,
                SA1 = 1, GCT = 2, DA1 = 0, SID = 0,
                ByteTransform = { DataFormat = HslCommunication.Core.DataFormat.CDAB, IsStringReverseByteWord = true }
            };
        }

        private void bgwCustomRead_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var plc = CreateCustomPlcClient())
                {
                    OperateResult<int> read = plc.ReadInt32(uiNumPadTextBox5.Text);
                    this.InvokeIfRequired(() => opValueCus.Text = read.IsSuccess ? read.Content.ToString() : "Lỗi đọc");
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khi kết nối PLC: {ex.Message}");
            }
        }

        private void bgwCustomWrite_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var plc = CreateCustomPlcClient())
                {
                    plc.Write(uiNumPadTextBox5.Text, int.Parse(ipValueCust.Text));
                    OperateResult<int> read = plc.ReadInt32(uiNumPadTextBox5.Text);
                    this.InvokeIfRequired(() => opValueCus.Text = read.IsSuccess ? read.Content.ToString() : "Lỗi đọc sau ghi");
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khi kết nối PLC: {ex.Message}");
            }
        }
        #endregion
    }

    #region RecipeManager Class
    public class RecipeManager
    {
        private readonly string _recipeDirectory;
        private readonly string _logFilePath;
        private readonly PLC_Parameter _defaultConfig;

        public PLC_Parameter CurrentRecipeOnPC { get; private set; }
        public string SelectedRecipeName { get; private set; }

        public RecipeManager(string directory, PLC_Parameter defaultConfig)
        {
            _recipeDirectory = directory;
            _logFilePath = Path.Combine(_recipeDirectory, "log.rlplc");
            _defaultConfig = defaultConfig;
        }

        public void Initialize()
        {
            if (!Directory.Exists(_recipeDirectory))
            {
                Directory.CreateDirectory(_recipeDirectory);
            }
            if (!File.Exists(_logFilePath))
            {
                CreateLogTable();
            }

            LoadLastSelectedRecipe();
        }

        private void LoadLastSelectedRecipe()
        {
            DataTable lastRecipeTable = GetLastActionFromLog("SELECT");
            if (lastRecipeTable.Rows.Count > 0)
            {
                SelectedRecipeName = lastRecipeTable.Rows[0]["RecipeName"].ToString();
                string recipePath = Path.Combine(_recipeDirectory, $"{SelectedRecipeName}.rplc");

                if (File.Exists(recipePath))
                {
                    string json = File.ReadAllText(recipePath);
                    CurrentRecipeOnPC = JsonConvert.DeserializeObject<PLC_Parameter>(json) ?? _defaultConfig;
                }
                else
                {
                    // If file is missing, revert to default
                    SelectRecipe("Default", _defaultConfig);
                }
            }
            else
            {
                SelectRecipe("Default", _defaultConfig);
            }
        }

        public void SelectRecipe(string recipeName, PLC_Parameter config = null)
        {
            SelectedRecipeName = recipeName;
            string recipePath = Path.Combine(_recipeDirectory, $"{recipeName}.rplc");

            if (config != null)
            {
                CurrentRecipeOnPC = config;
            }
            else if (File.Exists(recipePath))
            {
                string json = File.ReadAllText(recipePath);
                CurrentRecipeOnPC = JsonConvert.DeserializeObject<PLC_Parameter>(json);
            }
            else
            {
                CurrentRecipeOnPC = _defaultConfig;
                SaveRecipe(CurrentRecipeOnPC); // Create the file
            }

            AddLog("SELECT", RecipeToString(CurrentRecipeOnPC), GlobalVarialbles.CurrentUser.Username);
        }

        public void SaveRecipe(PLC_Parameter newConfig)
        {
            CurrentRecipeOnPC = newConfig;
            string json = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
            File.WriteAllText(Path.Combine(_recipeDirectory, $"{SelectedRecipeName}.rplc"), json);
            AddLog("UPDATE", RecipeToString(newConfig), GlobalVarialbles.CurrentUser.Username);
        }

        public string CreateNewRecipe(string newRecipeName, PLC_Parameter currentConfig)
        {
            string newRecipePath = Path.Combine(_recipeDirectory, $"{newRecipeName}.rplc");
            if (File.Exists(newRecipePath))
            {
                return "Tên Recipe đã tồn tại, vui lòng chọn tên khác.";
            }

            string json = JsonConvert.SerializeObject(currentConfig, Formatting.Indented);
            File.WriteAllText(newRecipePath, json);

            SelectedRecipeName = newRecipeName;
            AddLog("CREATE", RecipeToString(currentConfig), GlobalVarialbles.CurrentUser.Username);
            AddLog("SELECT", RecipeToString(currentConfig), GlobalVarialbles.CurrentUser.Username);

            return null; // Success
        }

        public bool DeleteRecipe(string recipeName)
        {
            if (recipeName.Equals("Default", StringComparison.OrdinalIgnoreCase)) return false;

            string filePath = Path.Combine(_recipeDirectory, $"{recipeName}.rplc");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                AddLog("DELETE", "N/A", GlobalVarialbles.CurrentUser.Username, recipeName);
                SelectRecipe("Default");
                return true;
            }
            return false;
        }

        private string RecipeToString(PLC_Parameter recipe) => $"{recipe.DelayCamera},{recipe.DelayReject},{recipe.RejectStreng}";

        #region SQLite Helpers
        private void CreateLogTable()
        {
            using (var conn = new SQLiteConnection($"Data Source={_logFilePath};Version=3;"))
            {
                conn.Open();
                const string createTableQuery = @"CREATE TABLE ""Log"" (
                    ""ID"" INTEGER NOT NULL UNIQUE,
                    ""RecipeName"" TEXT, ""RecipeValue"" TEXT, ""Action"" TEXT,
                    ""Timestamp"" TEXT, ""UserName"" TEXT, PRIMARY KEY(""ID"" AUTOINCREMENT))";
                using (var cmd = new SQLiteCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AddLog(string action, string recipeValue, string userName, string recipeName = null)
        {
            using (var conn = new SQLiteConnection($"Data Source={_logFilePath};Version=3;"))
            {
                conn.Open();
                const string insertQuery = @"INSERT INTO Log (RecipeName, RecipeValue, Action, Timestamp, UserName) 
                                             VALUES (@RecipeName, @RecipeValue, @Action, @Timestamp, @UserName)";
                using (var cmd = new SQLiteCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@RecipeName", recipeName ?? SelectedRecipeName);
                    cmd.Parameters.AddWithValue("@RecipeValue", recipeValue);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private DataTable GetLastActionFromLog(string action)
        {
            using (var connection = new SQLiteConnection($"Data Source={_logFilePath};Version=3;"))
            {
                connection.Open();
                string query = $"SELECT * FROM Log WHERE Action = @Action ORDER BY `ID` DESC LIMIT 1;";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Action", action);
                    using (var da = new SQLiteDataAdapter(command))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }
        #endregion
    }

    public class PLC_Parameter
    {
        public string DelayCamera { get; set; } = "0";
        public string DelayReject { get; set; } = "0";
        public string RejectStreng { get; set; } = "0";
    }
    #endregion
}
