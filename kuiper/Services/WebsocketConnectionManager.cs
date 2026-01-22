using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace kuiper.Services
{
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, PlayerData> _connections = new();
        private readonly ConcurrentDictionary<string, long> _connectionSlotMap = new();
        private readonly ILogger<WebSocketConnectionManager> _logger;

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        public async Task AddConnectionAsync(string connectionId, PlayerData playerData)
        {
            _connections.TryAdd(connectionId, playerData);
            
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var playerData))
            {
                if (playerData.Socket.State == WebSocketState.Open)
                {
                    await playerData.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
            }

            // unmap slot mapping when connection removed
            await UnmapConnectionAsync(connectionId);
        }

        public async Task BroadcastAsync(string message)
        {
            var tasks = new List<Task>();

            foreach (var connection in _connections)
            {
                if (connection.Value.Socket.State == WebSocketState.Open)
                {
                    tasks.Add(SendMessageToConnectionAsync(connection.Value.Socket, message));
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Send a text message to a specific connection by id. No-op if the connection does not exist or is not open.
        /// </summary>
        public async Task SendToConnectionAsync(string connectionId, string message)
        {
            if (string.IsNullOrEmpty(connectionId)) return;
            if (!_connections.TryGetValue(connectionId, out var playerData)) return;

            if (playerData.Socket.State != WebSocketState.Open) return;

            await SendMessageToConnectionAsync(playerData.Socket, message);
        }

        /// <summary>
        /// Serialize an object to JSON and send it to a specific connection by id.
        /// </summary>
        public async Task SendJsonToConnectionAsync<T>(string connectionId, T obj)
        {
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(obj, options);
            await SendToConnectionAsync(connectionId, json);
        }

        private async Task SendMessageToConnectionAsync(WebSocket webSocket, string message)
        {
            try
            {
                _logger.LogDebug("Begin sending: {message}", message);
                var bytes = Encoding.UTF8.GetBytes(message);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message");
            }
        }

        public int GetConnectionCount() => _connections.Count;

        // Mapping methods (in-memory only)
        public Task MapConnectionToSlotAsync(string connectionId, long slot)
        {
            if (string.IsNullOrEmpty(connectionId)) throw new ArgumentNullException(nameof(connectionId));
            _connectionSlotMap[connectionId] = slot;
            return Task.CompletedTask;
        }

        public Task<long?> GetSlotForConnectionAsync(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return Task.FromResult<long?>(null);
            if (_connectionSlotMap.TryGetValue(connectionId, out var s)) return Task.FromResult<long?>(s);
            return Task.FromResult<long?>(null);
        }

        public Task<IReadOnlyCollection<string>> GetConnectionIdsForSlotAsync(long slot)
        {
            var connectionIds = _connectionSlotMap
                .Where(kvp => kvp.Value == slot)
                .Select(kvp => kvp.Key)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<string>>(connectionIds);
        }

        public Task UnmapConnectionAsync(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId)) return Task.CompletedTask;
            _connectionSlotMap.TryRemove(connectionId, out _);
            return Task.CompletedTask;
        }

        public IReadOnlyCollection<string> GetAllConnectionIds()
        {
            return _connections.Keys.ToArray();
        }
    }
}
