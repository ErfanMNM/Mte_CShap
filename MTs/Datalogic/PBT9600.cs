using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTs.Datalogic
{
    public enum ePBT9600State
    {
        Connected,
        Disconnected,
        Received,
        Error
    }

    public sealed class PBT9600
    {
        #region Fields
        private SerialPort? _serialPort;
        #endregion

        #region Properties
        public string PortName { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;
        public bool Connected => _serialPort?.IsOpen ?? false;
        #endregion

        #region Events
        public delegate void SerialClientEventHandler(ePBT9600State state, string data);
        public event SerialClientEventHandler? SerialClientCallback;
        #endregion

        #region Constructors
        public PBT9600(string portName, int baudRate)
        {
            PortName = portName;
            BaudRate = baudRate;
        }
        #endregion

        #region Private Methods
        private void OnSerialClientCallback(ePBT9600State state, string data)
        {
            SerialClientCallback?.Invoke(state, data);
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen) return;
                int bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] buffer = new byte[bytesToRead];
                    _serialPort.Read(buffer, 0, bytesToRead);
                    string receivedData = Encoding.UTF8.GetString(buffer);
                    OnSerialClientCallback(ePBT9600State.Received, receivedData);
                }
            }
            catch (Exception ex)
            {
                HandleDisconnection($"Error receiving data: {ex.Message}");
            }
        }

        private void HandleDisconnection(string reason)
        {
            if (!Connected) return;

            CleanupPort();
            OnSerialClientCallback(ePBT9600State.Disconnected, reason);
        }

        private void CleanupPort()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort.Dispose();
                _serialPort = null;
            }
        }
        #endregion

        #region Public Methods
        public void Connect()
        {
            if (Connected)
            {
                Disconnect();
            }

            try
            {
                _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);
                _serialPort.DataReceived += DataReceivedHandler;
                _serialPort.Open();

                if (_serialPort.IsOpen)
                {
                    OnSerialClientCallback(ePBT9600State.Connected, "Connected successfully");
                }
                else
                {
                    OnSerialClientCallback(ePBT9600State.Disconnected, "Failed to open port");
                }
            }
            catch (Exception ex)
            {
                OnSerialClientCallback(ePBT9600State.Error, $"Connection failed: {ex.Message}");
            }
        }

        public void Disconnect()
        {
            HandleDisconnection("Disconnected by user.");
        }

        public async Task<bool> SendAsync(string data)
        {
            if (string.IsNullOrEmpty(data) || _serialPort == null || !Connected)
            {
                return false;
            }

            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                await _serialPort.BaseStream.WriteAsync(dataBytes, 0, dataBytes.Length);
                return true;
            }
            catch (Exception ex)
            {
                HandleDisconnection($"Send failed: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}
