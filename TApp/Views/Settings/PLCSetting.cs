
using HslCommunication;
using Newtonsoft.Json;
using Sunny.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TApp.Configs;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Utils;
using TTManager.Diaglogs;

namespace TApp.Views.Settings
{
    public partial class PLCSetting : UIPage
    {
        // Thêm biến BackgroundWorker
        private BackgroundWorker bgwSavePLC;
        private BackgroundWorker bgwSavePLCCS;
        private BackgroundWorker bgwUpdate;

        public PLCSetting()
        {
            InitializeComponent();

            // Khởi tạo BackgroundWorker cho lưu PLC
            bgwSavePLC = new BackgroundWorker();
            bgwSavePLC.DoWork += bgwSavePLC_DoWork;
            bgwSavePLC.RunWorkerCompleted += bgwSavePLC_RunWorkerCompleted;

            //backgroundWorker cập nhật thông số PLC
            bgwUpdate = new BackgroundWorker();
            bgwUpdate.WorkerSupportsCancellation = true;
            bgwUpdate.DoWork += PLC_Comfirm_Async;
        }

        public string SelectRecipeName = string.Empty;
        public string log_FilePath = "PLC_RECIPEs/log.rlplc";
        public string defaultFilePath = "PLC_RECIPEs/Default.rplc";
        public static PLC_Parameter PLC_Parameter_On_PC { get; set; } = new PLC_Parameter();
        public static PLC_Parameter PLC_Parameter_On_PLC { get; set; } = new PLC_Parameter();

        public bool isOpen { get; set; } = false;
        public bool isLoading { get; set; } = false;

        public string SelectRecipeName_CS = string.Empty;
        public string log_FilePath_CS = "PLC_RECIPEs_CS/log.rlplc";
        public string defaultFilePath_CS = "PLC_RECIPEs_CS/Default.rplc";
        public static PLC_Parameter PLC_Parameter_On_PC_CS { get; set; } = new PLC_Parameter();
        public static PLC_Parameter PLC_Parameter_On_PLC_CS { get; set; } = new PLC_Parameter();

        public void INIT()
        {
            isLoading = true;
            FirstCheck();
            FirstCheck_CS();
            omronplC_Hsl1.PLC_IP = AppConfigs.Current.PLC_IP;
            omronplC_Hsl1.PLC_PORT = AppConfigs.Current.PLC_Port;

            if(AppConfigs.Current.PLC_Test_Mode)
            {
                omronplC_Hsl1.PLC_IP = "127.0.0.1";
                omronplC_Hsl1.PLC_PORT = 9600;
            }

            omronplC_Hsl1.InitPLC();
            bgwUpdate.RunWorkerAsync();

            isLoading = false;
        }

        public void FirstCheck()
        {
            PLC_Parameter defaultConfig;
            defaultConfig = new PLC_Parameter
            {
                DelayCamera = "1000",
                DelayReject = "2000",
                RejectStreng = "20",
            };

            if (!Directory.Exists("PLC_RECIPEs"))
            {
                Directory.CreateDirectory("PLC_RECIPEs");
            }
            if (!File.Exists(log_FilePath))
            {
                SQLiteConnection.CreateFile(log_FilePath);
                CreateLogTable(log_FilePath);
            }
            if (!File.Exists(defaultFilePath))
            {
                //tạo file json mặc định
                File.WriteAllText(defaultFilePath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
               // return;
            }
            DataTable datatable = Get_Last_Select_Recipe();
            
            if (datatable.Rows.Count >= 1)
            {
                SelectRecipeName = datatable.Rows[0]["RecipeName"].ToString();
                string[] valueR = datatable.Rows[0]["RecipeValue"].ToString().Split(",");
                defaultConfig = new PLC_Parameter
                {
                    DelayCamera = valueR[0],
                    DelayReject = valueR[1],
                    RejectStreng = valueR[2]
                };
            }
            else
            {
                SelectRecipeName = "Default";
                defaultConfig = new PLC_Parameter
                {
                    DelayCamera = "1000",
                    DelayReject = "2000",
                    RejectStreng = "20",
                };
            }
            Check_Recipe(SelectRecipeName, defaultConfig);
        }

        public void FirstCheck_CS()
        {
            PLC_Parameter defaultConfig_CS;
            defaultConfig_CS = new PLC_Parameter
            {
                DelayCamera = "1000",
                DelayReject = "2000",
                RejectStreng = "20",
            };
            if (!Directory.Exists("PLC_RECIPEs_CS"))
            {
                Directory.CreateDirectory("PLC_RECIPEs_CS");
            }
            if (!File.Exists(log_FilePath_CS))
            {
                SQLiteConnection.CreateFile(log_FilePath_CS);
                CreateLogTable(log_FilePath_CS);
            }
            if (!File.Exists(defaultFilePath_CS))
            {
                //tạo file json mặc định
                File.WriteAllText(defaultFilePath_CS, JsonConvert.SerializeObject(defaultConfig_CS, Formatting.Indented));
            }
            DataTable datatable = Get_Last_Select_Recipe_CS();
            
            if (datatable.Rows.Count >= 1)
            {
                SelectRecipeName_CS = datatable.Rows[0]["RecipeName"].ToString();
                string[] valueR = datatable.Rows[0]["RecipeValue"].ToString().Split(",");
                defaultConfig_CS = new PLC_Parameter
                {
                    DelayCamera = valueR[0],
                    DelayReject = valueR[1],
                    RejectStreng = valueR[2]
                };
            }
            else
            {
                SelectRecipeName_CS = "Default";
                defaultConfig_CS = new PLC_Parameter
                {
                    DelayCamera = "1000",
                    DelayReject = "2000",
                    RejectStreng = "20",
                };
            }

        }

        private void uiTableLayoutPanel1_Paint(object sender, PaintEventArgs e) { }

        public void GetParameterFromPLC()
        {
            OperateResult<int[]> read = omronplC_Hsl1.plc.ReadInt32(PLCAddressWithGoogleSheetHelper.Get("PLC_Delay_Camera_DM_C2"), 3);
            if (read.IsSuccess)
            {
                PLC_Parameter_On_PLC.DelayCamera = read.Content[0].ToString();
                PLC_Parameter_On_PLC.DelayReject = read.Content[1].ToString();
                PLC_Parameter_On_PLC.RejectStreng = read.Content[2].ToString();
            }
            else
            {
                PLC_Parameter_On_PLC.DelayCamera = "-1";
                PLC_Parameter_On_PLC.DelayReject = "-1";
                PLC_Parameter_On_PLC.RejectStreng = "-1";
            }
        }

        PLC_Parameter defaultConfig = new PLC_Parameter
        {
            DelayCamera = "1000",
            DelayReject = "2000",
            RejectStreng = "20",
        };
        public void CreateDefaultRecipeTable()
        {
            string json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
            File.WriteAllText(defaultFilePath, json);
            SelectRecipeName = "Default";
            string RecipeValue = $"{defaultConfig.DelayCamera},{defaultConfig.DelayReject},{defaultConfig.RejectStreng}";
            AddLogRecipe("Default", RecipeValue, "CREATE", "Operator");
            AddLogRecipe("Default", RecipeValue, "SELECT", "Operator");
            PLC_Parameter_On_PC = defaultConfig;
        }
        public void Write_Recipe_To_File(string json)
        {
            File.WriteAllText($"PLC_RECIPEs/{SelectRecipeName}.rplc", json);
            AddLogRecipe(SelectRecipeName, json, "UPDATE", "Operator");
        }
        public void CreateLogTable(string Camera_Folder)
        {
            using (var conn = new SQLiteConnection($"Data Source={Camera_Folder};Version=3;"))
            {
                conn.Open();
                string createTableQuery = @"CREATE TABLE ""Log"" (
                                             ""ID""	INTEGER NOT NULL UNIQUE,
                                             ""RecipeName""	TEXT,
                                                ""RecipeValue""	TEXT,
                                             ""Action""	TEXT,
                                             ""Timestamp""	TEXT,
                                             ""UserName""	TEXT,
                                             PRIMARY KEY(""ID"" AUTOINCREMENT)
                                                )";
                using (var cmd = new SQLiteCommand(createTableQuery, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public void Check_Recipe(string recipeName, PLC_Parameter configs)
        {
            string[] files = Directory.GetFiles("PLC_RECIPEs", "*.rplc");
            if (files.Length == 0)
            {
                string json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                File.WriteAllText($"PLC_RECIPEs/{SelectRecipeName}.rplc", json);
                AddLogRecipe(SelectRecipeName, $"{defaultConfig.DelayCamera},{defaultConfig.DelayReject},{defaultConfig.RejectStreng}", "CREATE", "Operator");
                AddLogRecipe(SelectRecipeName, $"{defaultConfig.DelayCamera},{defaultConfig.DelayReject},{defaultConfig.RejectStreng}", "SELECT", "Operator");
                PLC_Parameter_On_PC = defaultConfig;
            }
            else
            {
                bool fileExists = files.Any(file => Path.GetFileName(file).Equals(SelectRecipeName, StringComparison.OrdinalIgnoreCase));
                if (!fileExists)
                {
                    string json = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
                    File.WriteAllText($"PLC_RECIPEs/{SelectRecipeName}.rplc", json);
                    AddLogRecipe(SelectRecipeName, $"{defaultConfig.DelayCamera},{defaultConfig.DelayReject},{defaultConfig.RejectStreng}", "CREATE", "Operator");
                    AddLogRecipe(SelectRecipeName, $"{defaultConfig.DelayCamera},{defaultConfig.DelayReject},{defaultConfig.RejectStreng}", "SELECT", "Operator");
                    PLC_Parameter_On_PC = defaultConfig;
                }
                else
                {
                    string jsonContent = File.ReadAllText($"PLC_RECIPEs/{SelectRecipeName}.rplc");
                    PLC_Parameter_On_PC = JsonConvert.DeserializeObject<PLC_Parameter>(jsonContent);
                }
            }
            this.InvokeIfRequired(() =>
            {
                ipDelayTriger.Text = PLC_Parameter_On_PC.DelayCamera;
                ipDelayReject.Text = PLC_Parameter_On_PC.DelayReject;
                ipRejectStreng.Text = PLC_Parameter_On_PC.RejectStreng;
            });
        }
        public void AddLogRecipe(string recipeName, string recipeValue, string action, string userName)
        {
            //kiểm tra file log tồn tại chưa nếu chưa tạo mới



            using (var conn = new SQLiteConnection($"Data Source={log_FilePath};Version=3;"))
            {
                conn.Open();
                string insertQuery = @"INSERT INTO Log (RecipeName, RecipeValue, Action, Timestamp, UserName) 
                                       VALUES (@RecipeName, @RecipeValue, @Action, @Timestamp, @UserName)";
                using (var cmd = new SQLiteCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@RecipeName", recipeName);
                    cmd.Parameters.AddWithValue("@RecipeValue", recipeValue);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@Timestamp", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public DataTable Get_Last_Select_Recipe()
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source=PLC_RECIPEs/log.rlplc;Version=3;"))
            {
                connection.Open();
                string query = $"SELECT * FROM Log WHERE Action = 'SELECT' ORDER BY `ID` DESC LIMIT 1;";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }


        public class PLC_Parameter
        {
            public string DelayCamera { get; set; } = "0";
            public string DelayReject { get; set; } = "0";
            public string RejectStreng { get; set; } = "0";
        }

        private void PLCSetting_Initialize(object sender, EventArgs e)
        {
            isOpen = true;
            Uri uri = new Uri($"http://{AppConfigs.Current.Camera_01_IP}/monitor");
            Uri uri1 = new Uri($"https://google.com");
            webView21.Source = uri;

            // Ghi log mở trang cài đặt PLC
            GlobalVarialbles.Logger?.WriteLogAsync(
                GlobalVarialbles.CurrentUser.Username,
                e_LogType.UserAction,
                "Mở trang cài đặt PLC",
                "",
                "UA-PLCSETTING-01"
            );
        }
        private void PLCSetting_Finalize(object sender, EventArgs e)
        {
            isOpen = false;
            webView21.Source = new Uri("https://google.com");
        }

        private void ipRecipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ipRecipe.SelectedText.Length > 3)
            {
                SelectRecipeName = ipRecipe.SelectedText;
                string jsonContent = File.ReadAllText($"PLC_RECIPEs/{SelectRecipeName}.rplc");
                PLC_Parameter az = JsonConvert.DeserializeObject<PLC_Parameter>(jsonContent);
                ipDelayTriger.Text = az.DelayCamera;
                ipDelayReject.Text = az.DelayReject;
                ipRejectStreng.Text = az.RejectStreng;
            }
            if (isLoading)
                return;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!bgwSavePLC.IsBusy)
                bgwSavePLC.RunWorkerAsync();
        }

        private void bgwSavePLC_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string delayCamera = ipDelayTriger.Text;
                string delayReject = ipDelayReject.Text;
                string rejectStreng = ipRejectStreng.Text;
                PLC_Parameter_On_PC.DelayCamera = delayCamera;
                PLC_Parameter_On_PC.DelayReject = delayReject;
                PLC_Parameter_On_PC.RejectStreng = rejectStreng;
                string json = JsonConvert.SerializeObject(PLC_Parameter_On_PC, Formatting.Indented);
                Write_Recipe_To_File(json);
                OperateResult operateResult = omronplC_Hsl1.plc.Write(PLCAddressWithGoogleSheetHelper.Get("PLC_Delay_Camera_DM_C2"), new int[] { int.Parse(delayCamera), int.Parse(delayReject), int.Parse(rejectStreng) });
                e.Result = operateResult;
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void bgwSavePLC_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is OperateResult operateResult)
            {
                if (!operateResult.IsSuccess)
                {
                    this.ShowErrorDialog($"Lỗi khi ghi vào PLC: {operateResult.Message}");
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.Error,
                        "Lỗi lưu cài đặt PLC",
                        operateResult.Message,
                        "ERR-PLCSETTING-01"
                    );
                }
                else
                {
                    this.ShowSuccessDialog("Cập nhật thành công!");
                    GlobalVarialbles.Logger?.WriteLogAsync(
                        GlobalVarialbles.CurrentUser.Username,
                        e_LogType.UserAction,
                        "Lưu cài đặt PLC thành công",
                        $"{{'DelayCamera':'{PLC_Parameter_On_PC.DelayCamera}','DelayReject':'{PLC_Parameter_On_PC.DelayReject}','RejectStreng':'{PLC_Parameter_On_PC.RejectStreng}'}}",
                        "UA-PLCSETTING-02"
                    );
                }
            }
            else if (e.Result is Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khi cập nhật: {ex.Message}");
                GlobalVarialbles.Logger?.WriteLogAsync(
                    GlobalVarialbles.CurrentUser.Username,
                    e_LogType.Error,
                    "Lỗi cập nhật PLC",
                    ex.Message,
                    "ERR-PLCSETTING-02"
                );
            }
        }

        private void btnUndo_Click(object sender, EventArgs e)
        {
            ipDelayReject.Text = PLC_Parameter_On_PLC.DelayReject;
            ipDelayTriger.Text = PLC_Parameter_On_PLC.DelayCamera;
            ipRejectStreng.Text = PLC_Parameter_On_PLC.RejectStreng;
        }

        private void btnNewRecipe_Click(object sender, EventArgs e)
        {
            using (Entertext enterText = new Entertext())
            {
                enterText.TileText = "Nhập tên Recipe";
                enterText.TextValue = "New Recipe";
                enterText.EnterClicked += (s, args) =>
                {
                    if (File.Exists($"PLC_RECIPEs/{enterText.TextValue}.rplc"))
                    {
                        this.ShowErrorDialog("Tên Recipe đã tồn tại, vui lòng chọn tên khác.");
                        return;
                    }
                    PLC_Parameter newRecipe = new PLC_Parameter
                    {
                        DelayCamera = PLC_Parameter_On_PC.DelayCamera,
                        DelayReject = PLC_Parameter_On_PC.DelayReject,
                        RejectStreng = PLC_Parameter_On_PC.RejectStreng
                    };
                    string json = JsonConvert.SerializeObject(newRecipe, Formatting.Indented);
                    File.WriteAllText($"PLC_RECIPEs/{enterText.TextValue}.rplc", json);
                    AddLogRecipe(enterText.TextValue , $"{newRecipe.DelayCamera},{newRecipe.DelayReject},{newRecipe.RejectStreng}", "CREATE", GlobalVarialbles.CurrentUser.Username);
                    AddLogRecipe(enterText.TextValue, $"{newRecipe.DelayCamera},{newRecipe.DelayReject},{newRecipe.RejectStreng}", "SELECT", GlobalVarialbles.CurrentUser.Username);
                    ipRecipe.Items.Add(enterText.TextValue);
                    SelectRecipeName = enterText.TextValue;
                    ipRecipe.SelectedItem = SelectRecipeName;
                    this.ShowSuccessDialog($"Đã tạo Recipe mới: {enterText.TextValue}");
                };
                enterText.ShowDialog();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectRecipeName) || SelectRecipeName == "Default")
            {
                this.ShowErrorDialog("Không thể xóa Recipe mặc định.");
                return;
            }
            string filePath = $"PLC_RECIPEs/{SelectRecipeName}.rplc";
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    ipRecipe.Items.Remove(SelectRecipeName);
                    SelectRecipeName = "Default";
                    ipRecipe.SelectedItem = SelectRecipeName;
                    PLC_Parameter_On_PC.DelayCamera = defaultConfig.DelayCamera;
                    PLC_Parameter_On_PC.DelayReject = defaultConfig.DelayReject;
                    PLC_Parameter_On_PC.RejectStreng = defaultConfig.DelayReject;
                    AddLogRecipe(SelectRecipeName, "N/A", "DELETE", GlobalVarialbles.CurrentUser.Username);
                    this.ShowSuccessDialog($"Đã xóa Recipe: {SelectRecipeName}");
                }
                catch (Exception ex)
                {
                    this.ShowErrorDialog($"Lỗi khi xóa Recipe: {ex.Message}");
                }
            }
            else
            {
                this.ShowErrorDialog("Recipe không tồn tại.");
            }
        }

        PLC_Parameter defaultConfig_CS = new PLC_Parameter
        {
            DelayCamera = "1000",
            DelayReject = "2000",
            RejectStreng = "20",
        };
        public void CreateDefaultRecipeTable_CS()
        {
            string json = JsonConvert.SerializeObject(defaultConfig_CS, Formatting.Indented);
            File.WriteAllText(defaultFilePath_CS, json);
            SelectRecipeName_CS = "Default";
            string RecipeValue = $"{defaultConfig_CS.DelayCamera},{defaultConfig_CS.DelayReject},{defaultConfig_CS.RejectStreng}";
            AddLogRecipe("Default", RecipeValue, "CREATE", "Operator");
            AddLogRecipe("Default", RecipeValue, "SELECT", "Operator");
            PLC_Parameter_On_PC_CS = defaultConfig_CS;
        }

        public DataTable Get_Last_Select_Recipe_CS()
        {
            using (SQLiteConnection connection = new SQLiteConnection($"Data Source=PLC_RECIPEs_CS/log.rlplc;Version=3;"))
            {
                connection.Open();
                string query = $"SELECT * FROM Log WHERE Action = 'SELECT' ORDER BY `ID` DESC LIMIT 1;";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataAdapter da = new SQLiteDataAdapter(command))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        return dt;
                    }
                }
            }
        }




        private void PLC_Comfirm_Async(object sender, DoWorkEventArgs e)
        {

            while (!bgwUpdate.CancellationPending)
            {
                if (isOpen)
                {

                    GetParameterFromPLC();
                    this.InvokeIfRequired(() =>
                    {

                        opDelayTriger.Text = PLC_Parameter_On_PLC.DelayCamera;
                        opDelayReject.Text = PLC_Parameter_On_PLC.DelayReject;
                        opRejectStreng.Text = PLC_Parameter_On_PLC.RejectStreng;
                    });
                }
                Thread.Sleep(500); // Cập nhật mỗi 0.5 giây
            }

        }

        private void uiSymbolButton1_Click(object sender, EventArgs e)
        {
            if(!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                this.ShowErrorDialog("Đang đọc dữ liệu, vui lòng chờ!");
            }


        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                HslCommunication.Profinet.Omron.OmronFinsUdp plc = new HslCommunication.Profinet.Omron.OmronFinsUdp();
                plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeUdpNet(ipCPLCIP.Text, ipCPLPort.Text.ToInt())
                {
                    ReceiveTimeOut = 10000,
                    SleepTime = 0,
                    SocketKeepAliveTime = -1,
                    IsPersistentConnection = true,
                };
                plc.PlcType = HslCommunication.Profinet.Omron.OmronPlcType.CSCJ;
                plc.SA1 = 1;
                plc.GCT = 2;
                plc.DA1 = 0;
                plc.SID = 0;
                plc.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.CDAB;
                plc.ByteTransform.IsStringReverseByteWord = true;
                OperateResult<int> read = plc.ReadInt32(uiNumPadTextBox5.Text);
                if (read.IsSuccess)
                {
                    opValueCus.Text = read.Content.ToString();
                }
                else
                {
                    opValueCus.Text = "Lỗi";
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khi kết nối PLC: {ex.Message}");

            }
        }

        private void uiSymbolButton2_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker2.IsBusy)
            {
                backgroundWorker2.RunWorkerAsync();
            }
            else
            {
                this.ShowErrorDialog("Đang ghi dữ liệu, vui lòng chờ!");
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                HslCommunication.Profinet.Omron.OmronFinsUdp plc = new HslCommunication.Profinet.Omron.OmronFinsUdp();
                plc.CommunicationPipe = new HslCommunication.Core.Pipe.PipeUdpNet(ipCPLCIP.Text, ipCPLPort.Text.ToInt())
                {
                    ReceiveTimeOut = 10000,
                    SleepTime = 0,
                    SocketKeepAliveTime = -1,
                    IsPersistentConnection = true,
                };
                plc.PlcType = HslCommunication.Profinet.Omron.OmronPlcType.CSCJ;
                plc.SA1 = 1;
                plc.GCT = 2;
                plc.DA1 = 0;
                plc.SID = 0;
                plc.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.CDAB;
                plc.ByteTransform.IsStringReverseByteWord = true;


                OperateResult write = plc.Write(uiNumPadTextBox5.Text, int.Parse(ipValueCust.Text));
                if (write.IsSuccess)
                {

                }
                else
                {

                }

                OperateResult<int> read = plc.ReadInt32(uiNumPadTextBox5.Text);
                if (read.IsSuccess)
                {
                    opValueCus.Text = read.Content.ToString();
                }
                else
                {
                    opValueCus.Text = "Lỗi";
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog($"Lỗi khi kết nối PLC: {ex.Message}");

            }
        }
    }
}
