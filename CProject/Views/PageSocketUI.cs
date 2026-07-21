using CProject.Module;
using Sunny.UI;
using System.Text.Json;
using TTManager.Communication.WebSocket;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CProject.Views
{
    public partial class PageSocketUI : UIPage
    {
        private WebSocketServerHelper? _server;
        private const int DefaultPort = 8080;
        private const string DefaultPath = "/ws";

        public static DataPoolModule _dataPool = new DataPoolModule();
        public PageSocketUI()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                _server = new WebSocketServerHelper(DefaultPort, DefaultPath);
                _server.WebSocketServerCallback += OnServerEvent;
                _server.WebSocketServerMessageCallback += OnServerMessage;
                _server.ClientSubscribed += (id, topic) =>
                    AppendLog($"SUB  {id} -> {topic}");
                _server.ClientUnsubscribed += (id, topic) =>
                    AppendLog($"UNSUB {id} -> {topic}");
                AppendLog($"Server ready on ws://localhost:{DefaultPort}{DefaultPath}");
                btnOpenClose.Text = "Mở server";
            }
            catch (Exception ex)
            {
                AppendLog($"Init error: {ex.Message}");
            }
        }

        private void btnOpenClose_Click(object? sender, EventArgs e)
        {
            if (_server == null) return;

            try
            {
                if (_server.Listening)
                {
                    _server.Stop();
                    btnOpenClose.Text = "Mở server";
                    AppendLog("Server stopped");
                }
                else
                {
                    _server.Start();
                    btnOpenClose.Text = "Đóng server";
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Toggle error: {ex.Message}");
            }
        }

        private async void btnSend_Click(object? sender, EventArgs e)
        {
            if (_server == null) return;
            string text = ipCommand.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(text)) return;

            try
            {
                int sent = await SendToTopicAsync(text);
                if (sent == 0)
                {
                    await _server.BroadcastAsync(text);
                    AppendLog($"Broadcast: {text}");
                }
                else
                {
                    AppendLog($"Topic '{txtTopic.Text.Trim()}' -> {sent} client(s): {text}");
                }
                ipCommand.Clear();
            }
            catch (Exception ex)
            {
                AppendLog($"Send error: {ex.Message}");
            }
        }

        private async Task<int> SendToTopicAsync(string data)
        {
            string topic = txtTopic.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(topic) ||
                string.Equals(topic, "topic name", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }
            return await _server!.SendToTopicAsync(topic, data);
        }

        private void OnServerEvent(WebSocketServerState state, string data)
        {
            AppendLog($"[{state}] {data}");
        }

        private void OnServerMessage(WebSocketServerMessage message)
        {
            if (message.State != WebSocketServerState.Received) return;

            string payload = message.Data?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(payload)) return;

            string? command = ExtractCommand(payload);
            if (command == null) return;
            string topic = message.Topics.FirstOrDefault() ?? string.Empty;
            AppendLog($"CMD from {message.ClientId}: {command} (topics=[{string.Join(",", message.Topics)}])");

            switch (topic)
            {
                case "pool":
                    string[] AM = payload.Split('|');
                    if (AM.Length >= 2)
                    {
                        switch (AM[0])
                        {
                            case "GET=>PATH":
                                //trả về thông tin Pool được gọi
                                DataPoolResultString rs = _dataPool.GetPoolPath(AM[1]);
                                string rp = $"{rs.Success}|{rs.Message}|{rs.Data}";
                                _ = _server!.SendToTopicAsync(topic,rp );
                                AppendLog($"Sent POOL -> {message.ClientId}");
                                break;
                        }
                    }
                    break;
            }
            
        }

        private static string? ExtractCommand(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) return null;
            string trimmed = payload.Trim();

            if (trimmed.StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(trimmed);
                    if (doc.RootElement.TryGetProperty("command", out var cmdEl))
                    {
                        return cmdEl.GetString();
                    }
                    if (doc.RootElement.TryGetProperty("action", out var actEl))
                    {
                        return actEl.GetString();
                    }
                }
                catch
                {
                    return null;
                }
            }

            int spaceIdx = trimmed.IndexOfAny(new[] { ' ', '\t', '\r', '\n' });
            return spaceIdx < 0 ? trimmed : trimmed.Substring(0, spaceIdx);
        }

        private void HandleRestartCommand(WebSocketServerMessage message)
        {
            if (_server == null) return;
            try
            {
                _ = _server.SendToClientAsync(message.ClientId, "OK restarting");
                if (_server.Listening)
                {
                    _server.Stop();
                    AppendLog("Server stopped for restart");
                }
                _server.Start();
                btnOpenClose.Text = "Đóng server";
                AppendLog("Server restarted");
            }
            catch (Exception ex)
            {
                AppendLog($"Restart error: {ex.Message}");
            }
        }

        private void HandleStopCommand(WebSocketServerMessage message)
        {
            if (_server == null) return;
            try
            {
                _ = _server.SendToClientAsync(message.ClientId, "OK stopping");
                if (_server.Listening)
                {
                    _server.Stop();
                    btnOpenClose.Text = "Mở server";
                    AppendLog("Server stopped by client command");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Stop error: {ex.Message}");
            }
        }

        private void AppendLog(string line)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), line);
                return;
            }
            opCommand.Items.Insert(0, $"{DateTime.Now:HH:mm:ss.fff} {line}");
            if (opCommand.Items.Count > 500)
            {
                opCommand.Items.RemoveAt(opCommand.Items.Count - 1);
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent == null && _server != null)
            {
                try { _server.Dispose(); } catch { }
                _server = null;
            }
        }
    }
}
