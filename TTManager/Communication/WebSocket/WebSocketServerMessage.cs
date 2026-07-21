using System;
using System.Collections.Generic;

namespace TTManager.Communication.WebSocket
{
    public sealed class WebSocketServerMessage
    {
        public Guid ClientId { get; }
        public WebSocketServerState State { get; }
        public string Data { get; }
        public IReadOnlyCollection<string> Topics { get; }
        public DateTime Timestamp { get; }

        public WebSocketServerMessage(
            Guid clientId,
            WebSocketServerState state,
            string data,
            IReadOnlyCollection<string> topics)
        {
            ClientId = clientId;
            State = state;
            Data = data;
            Topics = topics ?? Array.Empty<string>();
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] [{State}] {ClientId} topics=[{string.Join(",", Topics)}] {Data}";
        }
    }
}
