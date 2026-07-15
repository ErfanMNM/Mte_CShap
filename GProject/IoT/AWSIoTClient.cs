using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using Serilog;

namespace GProject.IoT
{
    /// <summary>
    /// AWS IoT Core MQTT Client - sử dụng MQTTnet 4.x async API
    /// </summary>
    public class AWSIoTClient : IDisposable
    {
        private readonly MqttFactory _mqttFactory;
        private IMqttClient? _mqttClient;
        private CancellationTokenSource? _healthCheckCts;
        private CancellationTokenSource? _sendLoopCts;
        private readonly ConcurrentQueue<AWSSendItem> _sendQueue;
        private readonly ConcurrentDictionary<string, int> _pendingMessages;

        private AWSConfig _config;
        private AWSIoTStatus _currentStatus;
        private string _lastError = "";

        public event EventHandler<AWSStatusEventArgs>? OnStatusChanged;
        public event EventHandler<AWSMessageEventArgs>? OnMessageReceived;
        public event EventHandler<AWSSendResultEventArgs>? OnMessageSent;

        public AWSIoTStatus Status => _currentStatus;
        public string LastError => _lastError;
        public bool IsConnected => _mqttClient?.IsConnected ?? false;

        public AWSIoTClient(AWSConfig config)
        {
            _mqttFactory = new MqttFactory();
            _config = config;
            _sendQueue = new ConcurrentQueue<AWSSendItem>();
            _pendingMessages = new ConcurrentDictionary<string, int>();
            _currentStatus = AWSIoTStatus.Disconnected;
        }

        public void UpdateConfig(AWSConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Kết nối tới AWS IoT Core
        /// </summary>
        public async Task<bool> ConnectAsync()
        {
            if (_mqttClient?.IsConnected == true)
            {
                RaiseStatusChanged(AWSIoTStatus.Connected, "Đã kết nối");
                return true;
            }

            try
            {
                RaiseStatusChanged(AWSIoTStatus.Connecting, "Đang kết nối AWS IoT...");

                _mqttClient = _mqttFactory.CreateMqttClient();

                // Set up message received handler
                _mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    var topic = e.ApplicationMessage.Topic;
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                    OnMessageReceived?.Invoke(this, new AWSMessageEventArgs(topic, payload));
                    return Task.CompletedTask;
                };

                // Build options - port 8883 uses TLS automatically for AWS IoT
                var options = new MqttClientOptionsBuilder()
                    .WithTcpServer(_config.Endpoint, 8883)
                    .WithClientId(_config.ClientId)
                    .WithCredentials(_config.ClientId, "")
                    .WithProtocolVersion(MqttProtocolVersion.V311)
                    .WithCleanSession(true)
                    .Build();

                _mqttClient.DisconnectedAsync += e =>
                {
                    _currentStatus = AWSIoTStatus.Disconnected;
                    RaiseStatusChanged(AWSIoTStatus.Disconnected, "Mất kết nối AWS IoT");
                    
                    // Auto reconnect
                    if (_healthCheckCts?.IsCancellationRequested == false)
                    {
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(5000);
                            await ConnectAsync();
                        });
                    }
                    return Task.CompletedTask;
                };

                var response = await _mqttClient.ConnectAsync(options, CancellationToken.None);

                if (response.ResultCode == MqttClientConnectResultCode.Success)
                {
                    _currentStatus = AWSIoTStatus.Connected;
                    RaiseStatusChanged(AWSIoTStatus.Connected, "Kết nối thành công");
                    StartHealthCheck();
                    StartSendLoop();
                    return true;
                }
                else
                {
                    _lastError = $"Kết nối thất bại: {response.ResultCode}";
                    RaiseStatusChanged(AWSIoTStatus.Error, _lastError);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                RaiseStatusChanged(AWSIoTStatus.Error, $"Lỗi kết nối: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ngắt kết nối
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                StopHealthCheck();
                StopSendLoop();

                if (_mqttClient?.IsConnected == true)
                {
                    await _mqttClient.DisconnectAsync();
                }

                RaiseStatusChanged(AWSIoTStatus.Disconnected, "Đã ngắt kết nối");
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                RaiseStatusChanged(AWSIoTStatus.Error, $"Lỗi ngắt kết nối: {ex.Message}");
            }
        }

        /// <summary>
        /// Subscribe một topic
        /// </summary>
        public async Task<bool> SubscribeAsync(string topic, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            if (_mqttClient?.IsConnected != true)
            {
                RaiseStatusChanged(AWSIoTStatus.Unsubscribed, "Chưa kết nối, không thể subscribe");
                return false;
            }

            try
            {
                var subscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(topic, qos)
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);
                RaiseStatusChanged(AWSIoTStatus.Subscribed, $"Đã subscribe: {topic}");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                RaiseStatusChanged(AWSIoTStatus.Error, $"Lỗi subscribe: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Subscribe nhiều topics
        /// </summary>
        public async Task<bool> SubscribeMultipleAsync(string[] topics, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            if (_mqttClient?.IsConnected != true)
                return false;

            try
            {
                var builder = _mqttFactory.CreateSubscribeOptionsBuilder();
                foreach (var topic in topics)
                {
                    builder.WithTopicFilter(topic, qos);
                }
                var options = builder.Build();

                await _mqttClient.SubscribeAsync(options, CancellationToken.None);
                RaiseStatusChanged(AWSIoTStatus.Subscribed, $"Đã subscribe: {string.Join(", ", topics)}");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                RaiseStatusChanged(AWSIoTStatus.Error, $"Lỗi subscribe: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Publish message (đưa vào queue)
        /// </summary>
        public void Publish(string topic, string payload, bool retain = false)
        {
            var item = new AWSSendItem
            {
                Topic = topic,
                Payload = payload,
                CreatedAt = DateTime.UtcNow
            };
            _sendQueue.Enqueue(item);
        }

        /// <summary>
        /// Publish message đồng bộ (không qua queue)
        /// </summary>
        public async Task<(bool success, string message)> PublishAsync(string topic, string payload, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce, bool retain = false)
        {
            if (_mqttClient?.IsConnected != true)
            {
                return (false, "Chưa kết nối AWS IoT");
            }

            try
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(qos)
                    .WithRetainFlag(retain)
                    .Build();

                var response = await _mqttClient.PublishAsync(message, CancellationToken.None);

                // Check if the message was successfully delivered (no error = success)
                RaiseStatusChanged(AWSIoTStatus.Published, $"Đã gửi: {topic}");
                OnMessageSent?.Invoke(this, new AWSSendResultEventArgs(topic, payload, true, ""));
                return (true, "Gửi thành công");
            }
            catch (Exception ex)
            {
                var errorMsg = $"Exception: {ex.Message}";
                RaiseStatusChanged(AWSIoTStatus.Error, errorMsg);
                OnMessageSent?.Invoke(this, new AWSSendResultEventArgs(topic, payload, false, errorMsg));
                return (false, errorMsg);
            }
        }

        /// <summary>
        /// Publish AWSSendPayload
        /// </summary>
        public void PublishPayload(string topic, AWSSendPayload payload)
        {
            var json = JsonSerializer.Serialize(payload);
            Publish(topic, json);
        }

        /// <summary>
        /// Publish Carton Payload
        /// </summary>
        public void PublishCartonPayload(string topic, AWSCartonPayload payload)
        {
            var json = JsonSerializer.Serialize(payload);
            Publish(topic, json);
        }

        /// <summary>
        /// Publish PO Payload
        /// </summary>
        public void PublishPOPayload(string topic, AWSPOPayload payload)
        {
            var json = JsonSerializer.Serialize(payload);
            Publish(topic, json);
        }

        /// <summary>
        /// Số lượng message đang chờ gửi
        /// </summary>
        public int PendingCount => _sendQueue.Count;

        private void StartHealthCheck()
        {
            StopHealthCheck();
            _healthCheckCts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_healthCheckCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(5000, _healthCheckCts.Token);

                        if (_mqttClient?.IsConnected != true)
                        {
                            RaiseStatusChanged(AWSIoTStatus.Disconnected, "Mất kết nối, đang reconnect...");
                            await ConnectAsync();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("AWS HealthCheck lỗi: {Error}", ex.Message);
                    }
                }
            });
        }

        private void StopHealthCheck()
        {
            _healthCheckCts?.Cancel();
            _healthCheckCts?.Dispose();
            _healthCheckCts = null;
        }

        private void StartSendLoop()
        {
            StopSendLoop();
            _sendLoopCts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_sendLoopCts.Token.IsCancellationRequested)
                {
                    try
                    {
                        if (_sendQueue.TryDequeue(out var item))
                        {
                            var (success, msg) = await PublishAsync(item.Topic, item.Payload);

                            if (!success && item.RetryCount < 3)
                            {
                                item.RetryCount++;
                                _sendQueue.Enqueue(item);
                                await Task.Delay(1000, _sendLoopCts.Token);
                            }
                        }
                        else
                        {
                            await Task.Delay(100, _sendLoopCts.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning("AWS SendLoop lỗi: {Error}", ex.Message);
                        await Task.Delay(1000, _sendLoopCts.Token);
                    }
                }
            });
        }

        private void StopSendLoop()
        {
            _sendLoopCts?.Cancel();
            _sendLoopCts?.Dispose();
            _sendLoopCts = null;
        }

        private void RaiseStatusChanged(AWSIoTStatus status, string message)
        {
            _currentStatus = status;
            OnStatusChanged?.Invoke(this, new AWSStatusEventArgs(status, message));
        }

        public void Dispose()
        {
            DisconnectAsync().GetAwaiter().GetResult();
            _mqttClient?.Dispose();
            _sendQueue.Clear();
            _pendingMessages.Clear();
        }
    }

    /// <summary>
    /// Event args cho status change
    /// </summary>
    public class AWSStatusEventArgs : EventArgs
    {
        public AWSIoTStatus Status { get; }
        public string Message { get; }

        public AWSStatusEventArgs(AWSIoTStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    /// <summary>
    /// Event args cho message received
    /// </summary>
    public class AWSMessageEventArgs : EventArgs
    {
        public string Topic { get; }
        public string Payload { get; }

        public AWSMessageEventArgs(string topic, string payload)
        {
            Topic = topic;
            Payload = payload;
        }
    }

    /// <summary>
    /// Event args cho send result
    /// </summary>
    public class AWSSendResultEventArgs : EventArgs
    {
        public string Topic { get; }
        public string Payload { get; }
        public bool Success { get; }
        public string Message { get; }

        public AWSSendResultEventArgs(string topic, string payload, bool success, string message)
        {
            Topic = topic;
            Payload = payload;
            Success = success;
            Message = message;
        }
    }
}
