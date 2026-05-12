using System;
using System.Threading;
using System.Threading.Tasks;
using LibUA;
using LibUA.Core;

namespace TTManager.Communication.OPCUA
{
    public enum OpcUaClientState
    {
        Connected,
        Disconnected,
        Received,
        Error
    }

    public sealed class OpcUaClientHelper : IDisposable
    {
        #region Fields
        private Client? _client;
        private readonly SemaphoreSlim _syncLock = new(1, 1);
        private bool _disposed;
        #endregion

        #region Properties
        public string EndpointUrl { get; set; } = "opc.tcp://localhost:4840";
        public string SessionName { get; set; } = "TTManager OPC UA Client";
        public string ApplicationName { get; set; } = "TTManager";
        public string ApplicationUri { get; set; } = "urn:TTManager:OpcUaClient";
        public int Timeout { get; set; } = 10000;
        public int MaximumMessageSize { get; set; } = 65536;
        public int RequestedSessionTimeout { get; set; } = 60000;
        public bool Connected => _client?.IsConnected ?? false;
        #endregion

        #region Events
        public delegate void OpcUaClientEventHandler(OpcUaClientState state, string data);
        public event OpcUaClientEventHandler? OpcUaClientCallback;
        #endregion

        #region Constructors
        public OpcUaClientHelper(string endpointUrl)
        {
            EndpointUrl = endpointUrl;
        }
        #endregion

        #region Private Methods
        private void OnOpcUaClientCallback(OpcUaClientState state, string data)
        {
            OpcUaClientCallback?.Invoke(state, data);
        }

        private static NodeId ParseNodeId(string nodeId)
        {
            NodeId? parsed = NodeId.TryParse(nodeId);
            if (parsed == null || parsed.IsNull())
            {
                throw new ArgumentException($"Invalid node id: {nodeId}", nameof(nodeId));
            }

            return parsed;
        }

        private static Client CreateClient(string endpointUrl, int timeout, int maximumMessageSize)
        {
            if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out Uri? uri))
            {
                throw new ArgumentException($"Invalid endpoint url: {endpointUrl}", nameof(endpointUrl));
            }

            if (!string.Equals(uri.Scheme, "opc.tcp", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("EndpointUrl must use opc.tcp scheme.", nameof(endpointUrl));
            }

            string path = uri.AbsolutePath == "/" ? string.Empty : uri.AbsolutePath.TrimStart('/');
            return string.IsNullOrEmpty(path)
                ? new Client(uri.Host, uri.Port, timeout, maximumMessageSize)
                : new Client(uri.Host, uri.Port, path, timeout, maximumMessageSize);
        }

        private ApplicationDescription CreateApplicationDescription()
        {
            return new ApplicationDescription(
                ApplicationUri,
                ApplicationUri,
                new LocalizedText(ApplicationName),
                ApplicationType.Client,
                string.Empty,
                string.Empty,
                Array.Empty<string>());
        }

        private void CheckStatus(StatusCode statusCode, string action)
        {
            if (statusCode != StatusCode.Good)
            {
                throw new InvalidOperationException($"{action} failed: {statusCode}");
            }
        }

        private async Task<StatusCode> RunOpcUaStepAsync(Func<StatusCode> action, string actionName)
        {
            Task<StatusCode> actionTask = Task.Run(action);
            Task timeoutTask = Task.Delay(Timeout);
            Task completedTask = await Task.WhenAny(actionTask, timeoutTask);

            if (completedTask != actionTask)
            {
                throw new TimeoutException($"{actionName} timeout after {Timeout} ms.");
            }

            return await actionTask;
        }

        private void HandleDisconnection(string reason)
        {
            CleanupClient();
            OnOpcUaClientCallback(OpcUaClientState.Disconnected, reason);
        }

        private void CleanupClient()
        {
            if (_client == null) return;

            try
            {
                if (_client.IsConnected)
                {
                    _client.CloseSession();
                    _client.CloseSecureChannel();
                    _client.Disconnect();
                }
            }
            catch
            {
            }
            finally
            {
                _client.Dispose();
                _client = null;
            }
        }

        private void ForceDisposeClient()
        {
            try
            {
                _client?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _client = null;
            }
        }
        #endregion

        #region Public Methods
        public async Task ConnectAsync()
        {
            await _syncLock.WaitAsync();
            try
            {
                if (Connected)
                {
                    CleanupClient();
                }

                _client = CreateClient(EndpointUrl, Timeout, MaximumMessageSize);
                _client.OnConnectionClosed += () => HandleDisconnection("Connection closed by server.");

                OnOpcUaClientCallback(OpcUaClientState.Received, "Step 1/4: TCP connect...");
                CheckStatus(await RunOpcUaStepAsync(() => _client.Connect(), "TCP connect"), "Connect");

                OnOpcUaClientCallback(OpcUaClientState.Received, "Step 2/4: Open secure channel...");
                CheckStatus(await RunOpcUaStepAsync(() => _client.OpenSecureChannel(MessageSecurityMode.None, SecurityPolicy.None, null), "Open secure channel"), "Open secure channel");

                OnOpcUaClientCallback(OpcUaClientState.Received, "Step 3/4: Create session...");
                CheckStatus(await RunOpcUaStepAsync(() => _client.CreateSession(CreateApplicationDescription(), SessionName, RequestedSessionTimeout), "Create session"), "Create session");

                OnOpcUaClientCallback(OpcUaClientState.Received, "Step 4/4: Activate anonymous session...");
                CheckStatus(await RunOpcUaStepAsync(() => _client.ActivateSession(new UserIdentityAnonymousToken("anonymous"), Array.Empty<string>()), "Activate session"), "Activate session");

                OnOpcUaClientCallback(OpcUaClientState.Connected, "Connected successfully");
            }
            catch (Exception ex)
            {
                ForceDisposeClient();
                OnOpcUaClientCallback(OpcUaClientState.Error, $"Connection failed: {ex.Message}");
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public async Task DisconnectAsync()
        {
            await _syncLock.WaitAsync();
            try
            {
                HandleDisconnection("Disconnected by user.");
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public async Task<DataValue?> ReadAsync(string nodeId)
        {
            await _syncLock.WaitAsync();
            try
            {
                if (_client == null || !Connected) return null;

                ReadValueId[] requests =
                {
                    new(ParseNodeId(nodeId), NodeAttribute.Value, string.Empty, new QualifiedName(string.Empty))
                };

                DataValue[] results = Array.Empty<DataValue>();
                CheckStatus(await Task.Run(() => _client.Read(requests, out results)), "Read");
                DataValue? value = results.Length > 0 ? results[0] : null;
                OnOpcUaClientCallback(OpcUaClientState.Received, value?.Value?.ToString() ?? string.Empty);
                return value;
            }
            catch (Exception ex)
            {
                OnOpcUaClientCallback(OpcUaClientState.Error, $"Read failed: {ex.Message}");
                return null;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public async Task<object?> ReadValueAsync(string nodeId)
        {
            DataValue? dataValue = await ReadAsync(nodeId);
            return dataValue?.Value;
        }

        public async Task<bool> WriteAsync(string nodeId, object value)
        {
            await _syncLock.WaitAsync();
            try
            {
                if (_client == null || !Connected) return false;

                WriteValue[] requests =
                {
                    new(ParseNodeId(nodeId), NodeAttribute.Value, string.Empty, new DataValue(value, StatusCode.Good, DateTime.UtcNow, DateTime.UtcNow))
                };

                uint[] results = Array.Empty<uint>();
                CheckStatus(await Task.Run(() => _client.Write(requests, out results)), "Write");
                if (results.Length > 0 && results[0] != (uint)StatusCode.Good)
                {
                    OnOpcUaClientCallback(OpcUaClientState.Error, $"Write failed: {(StatusCode)results[0]}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                OnOpcUaClientCallback(OpcUaClientState.Error, $"Write failed: {ex.Message}");
                return false;
            }
            finally
            {
                _syncLock.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            CleanupClient();
            _syncLock.Dispose();
            _disposed = true;
        }
        #endregion
    }
}
