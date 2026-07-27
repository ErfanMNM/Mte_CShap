using LibUA;
using LibUA.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Sunny.UI;
using System.Data;
using System.Net;
using TApp.Helpers;
using TApp.Infrastructure;
using TApp.Views.Dashboard;
using TTManager.Masan;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Timers.Timer;

namespace TApp.Views
{
    public partial class Page_OPC_Production_Order : UIPage
    {
        private Client? _client;
        private readonly string _serverUrl = "opc.tcp://DESKTOP-3LR82CB:53530/OPCUA/SimulationServer";
        private const int Timeout = 10000;
        private const int MaximumMessageSize = 65536;
        private Timer? _pollTimer;
        private readonly List<NodeId> _monitoredNodes = new();

        public Page_OPC_Production_Order()
        {
            InitializeComponent();
        }

        private void Log(string message)
        {
            Invoke(() =>
            {
                uiListBox1.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
                if (uiListBox1.Items.Count > 50)
                {
                    uiListBox1.Items.RemoveAt(0);
                }
            }
            );
        }

        public async void Start()
        {
            if (_client != null && _client.IsConnected)
            {
                Log("Already connected");
                return;
            }

            try
            {
                Log($"Connecting to {_serverUrl}...");

                if (!Uri.TryCreate(_serverUrl, UriKind.Absolute, out Uri? uri))
                {
                    Log("Invalid endpoint URL");
                    return;
                }

                string path = uri.AbsolutePath == "/" ? string.Empty : uri.AbsolutePath.TrimStart('/');
                _client = string.IsNullOrEmpty(path)
                    ? new Client(uri.Host, uri.Port, Timeout, MaximumMessageSize)
                    : new Client(uri.Host, uri.Port, path, Timeout, MaximumMessageSize);

                _client.OnConnectionClosed += () => Log("Connection closed by server");

                Log("Step 1/4: TCP connect...");
                await Task.Run(() => _client.Connect());
                Log("Connected");

                Log("Step 2/4: Open secure channel...");
                await Task.Run(() => _client.OpenSecureChannel(MessageSecurityMode.None, SecurityPolicy.None, null));
                Log("Secure channel opened");

                Log("Step 3/4: Create session...");
                var appDesc = new ApplicationDescription(
                    "urn:MHG_Cartoning:OpcUaClient",
                    "urn:MHG_Cartoning:OpcUaClient",
                    new LocalizedText("MHG_Cartoning"),
                    ApplicationType.Client,
                    string.Empty,
                    string.Empty,
                    Array.Empty<string>());
                await Task.Run(() => _client.CreateSession(appDesc, "MHG_Cartoning_Session", 60000));
                Log("Session created");

                Log("Step 4/4: Activate session...");
                await Task.Run(() => _client.ActivateSession(new UserIdentityAnonymousToken("anonymous"), Array.Empty<string>()));
                Log("Session activated");

                Log("Connected successfully!");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }

            if (_client == null || !_client.IsConnected)
            {
                Log("Not connected. Please connect first.");
                return;
            }

            try
            {
                Log("Creating subscription...");

                // Add node to monitor
                var nodeId = NodeId.TryParse("ns=3;s=2001");
                var nodeId1 = NodeId.TryParse("ns=3;s=2002");
                if (nodeId != null)
                {
                    _monitoredNodes.Add(nodeId);
                    _monitoredNodes.Add(nodeId1);
                }

                // Start polling (LibUA doesn't have native subscription, use polling instead)
                StartPolling(1000);
            }
            catch (Exception ex)
            {
                Log($"Subscribe error: {ex.Message}");
            }

            if(!WK_UpServer.IsBusy)
            {
                WK_UpServer.RunWorkerAsync();
            }
        }

        private void StartPolling(int intervalMs = 1000)
        {
            _pollTimer?.Stop();
            _pollTimer?.Dispose();

            _pollTimer = new Timer(intervalMs);
            _pollTimer.Elapsed += async (s, e) => await PollValuesAsync();
            _pollTimer.Start();
        }

        private async Task PollValuesAsync()
        {
            if (_client == null || !_client.IsConnected || _monitoredNodes.Count == 0)
                return;

            try
            {
                var requests = _monitoredNodes.Select(n =>
                    new ReadValueId(n, NodeAttribute.Value, string.Empty, new QualifiedName(string.Empty))
                ).ToArray();

                DataValue[] results = Array.Empty<DataValue>();
                await Task.Run(() => _client.Read(requests, out results));

                for (int i = 0; i < results.Length && i < _monitoredNodes.Count; i++)
                {
                    if (results[i].Value != null)
                    {
                        Log($"{_monitoredNodes[i]} = {results[i].Value}");

                        if (GlobalVarialbles.CurrentAppState == e_AppState.Loading_PO)
                        {
                            FD_Globals.productionData.POItem = results[0].Value.ToString();
                            FD_Globals.productionData.POLot = results[0].Value.ToString();
                        }
                        else
                        {
                            bool PO_Change = false;
                            if (FD_Globals.productionData.POItem != results[0].Value.ToString())
                            {
                                FD_Globals.productionData.POItem = results[0].Value.ToString();
                                PO_Change = true;
                            }

                            if (FD_Globals.productionData.POLot != results[0].Value.ToString())
                            {
                                FD_Globals.productionData.POLot = results[0].Value.ToString();
                                PO_Change = true;
                            }

                            if (PO_Change)
                            {
                                GlobalVarialbles.CurrentAppState = e_AppState.Switching;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Poll error: {ex.Message}");
            }
        }

        private async void uiSymbolButton3_Click(object sender, EventArgs e)
        {
            if (_client == null || !_client.IsConnected)
            {
                Log("Not connected. Please connect first.");
                return;
            }

            try
            {
                var value = $"test+{DateTime.Now:HHmmss}";
                Log($"Writing \"{value}\" to ns=3;s=2001 and ns=3;s=2002...");

                var nodeId1 = NodeId.TryParse("ns=3;s=2001");
                var nodeId2 = NodeId.TryParse("ns=3;s=2002");

                if (nodeId1 == null || nodeId2 == null)
                {
                    Log("Invalid NodeId");
                    return;
                }

                var requests = new WriteValue[]
                {
                    new(nodeId1, NodeAttribute.Value, string.Empty,
                        new DataValue(ipPOItem.Text, StatusCode.Good, DateTime.UtcNow, DateTime.UtcNow)),
                    new(nodeId2, NodeAttribute.Value, string.Empty,
                        new DataValue(ipPOLot.Text, StatusCode.Good, DateTime.UtcNow, DateTime.UtcNow))
                };

                uint[] results = Array.Empty<uint>();
                await Task.Run(() => _client.Write(requests, out results));

                if (results.Length >= 2)
                {
                    var status1 = (StatusCode)results[0];
                    var status2 = (StatusCode)results[1];
                    Log($"Write 2001: {status1}, Write 2002: {status2}");
                }
            }
            catch (Exception ex)
            {
                Log($"Write error: {ex.Message}");
            }
        }

        private void WK_UpServer_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            int push = 0;
            while (!WK_UpServer.CancellationPending)
            {
                push++;
                Thread.Sleep(100);
                if (push > 1000)
                {
                    GlobalVarialbles.IsPush = true;
                }
                //lấy danh sách cần gửi
                if (GlobalVarialbles.IsPush)
                {
                  TResult result = QRDatabaseHelper.Get_ActiveQR_By_Push();

                    if (result.issuccess)
                    {
                        if (result.data.Rows.Count > 0)
                        {
                            foreach (DataRow ActiveUniQueCode in result.data.Rows)
                            {

                                
                                //Gửi từng dòng 1
                                string packageType = "2";
                                string cartonQRCode = ActiveUniQueCode["QRContent"].ToString();
                                string pucharseOrder = ActiveUniQueCode["POLot"].ToString();
                                string Status = "1";
                                var httpStatus = Pushing(packageType, cartonQRCode, pucharseOrder, Status);
                                Log($"Push to server: HTTP {(int)httpStatus}");
                                e.Result = httpStatus;
                                //Lưu ngược trạng thái vào file DB bằng luồng bên FDashboard
                                Thread.Sleep(1000);

                            }
                        }
                    }
                }
            }
        }

        private HttpStatusCode Pushing(
            string packageType,
            string cartonQRCode,
            string pucharseOrder,
            string Status,
            string baseUrl = "http://localhost:6969")
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "PushServer_Error.log");
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);

                    var data = new Dictionary<string, string>
                    {
                        { "packageType", packageType },
                        { "cartonQRCode", cartonQRCode },
                        { "pucharseOrder", pucharseOrder },
                        { "Status", Status }
                    };

                    var content = new FormUrlEncodedContent(data);
                    var response = client.PostAsync(baseUrl, content).GetAwaiter().GetResult();
                    return response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                Log($"Pushing error: {ex.Message}");

                // Ghi log lỗi vào file
                try
                {
                    var logDir = Path.GetDirectoryName(logFilePath);
                    if (!Directory.Exists(logDir))
                        Directory.CreateDirectory(logDir!);

                    string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR | QR:{cartonQRCode} | Msg:{ex.Message}\n";
                    File.AppendAllText(logFilePath, logEntry);
                }
                catch { }

                return HttpStatusCode.BadRequest;
            }
        }
    }
}
