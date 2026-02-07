using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class WebSocketConnectionManager : IConnectionManager
    {
        private readonly ILogger<WebSocketConnectionManager> _logger;
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ConcurrentDictionary<long, string[]> _slotsMapping = new();

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        public Task AddConnectionAsync(string connectionId, WebSocket socket)
        {
            if (_connections.TryAdd(connectionId, socket))
            {
                _logger.LogDebug("Added new connection to manager: {ConnectionId}", connectionId);
                return Task.CompletedTask; 
            }

            throw new InvalidOperationException("That Connection ID has already been added to the manager.");
        }
        public Task RemoveConnectionAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var socket))
            {
                UnmapConnectionAsync(connectionId);
                _logger.LogDebug("Removed connection from the manager: {ConnectionId}", connectionId);
                return Task.CompletedTask;
            }

            throw new InvalidOperationException("That Connection ID has not been added to the manager.");
        }

        public async Task SendJsonToConnectionAsync<T>(string connectionId, T obj)
        {
            var json = JsonSerializer.Serialize(obj, Json.NetworkDefaultOptions);
            await SendToConnectionAsync(connectionId, json);
        }

        public async Task SendToConnectionAsync(string connectionId, string message)
        {
            if (!_connections.TryGetValue(connectionId, out var socket))
            {
                throw new InvalidOperationException("Cannot send message to unknown connection. Add connection to manager first.");
            }

            if (socket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("The socket for the given connection is not open.");
            }

            try
            {
                _logger.LogDebug("Begin sending to {ConnectionId}: {message}", connectionId, message);
                var bytes = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to {ConnectionId}", connectionId);
                throw;
            }
        }

        public Task<IReadOnlyCollection<string>> GetConnectionIdsForSlotAsync(long slot)
        {
            if (_slotsMapping.TryGetValue(slot, out var connectionIds))
            {
                return Task.FromResult(connectionIds.AsReadOnly() as IReadOnlyCollection<string>);
            }
            return Task.FromResult(Array.Empty<string>().AsReadOnly() as IReadOnlyCollection<string>);
        }

        public Task<long?> GetSlotForConnectionAsync(string connectionId)
        {
            foreach (var slot in _slotsMapping.Keys)
            {
                if (_slotsMapping[slot].Contains(connectionId))
                {
                    return Task.FromResult(slot as long?);
                }
            }
            return Task.FromResult<long?>(null);
        }

        public Task MapConnectionToSlotAsync(string connectionId, long slot)
        {
            if (_slotsMapping.TryGetValue(slot, out var connectionIds))
            { 
                _slotsMapping[slot] = [..connectionIds, connectionId];
            }
            else
            {
                _slotsMapping.TryAdd(slot, [connectionId]);
            }

            _logger.LogDebug("{ConnectionId} mapped to slot {Slot}", connectionId, slot);
            return Task.CompletedTask;
        }

        public Task UnmapConnectionAsync(string connectionId)
        {
            foreach (var slot in _slotsMapping.Keys)
            {
                if (_slotsMapping[slot].Contains(connectionId))
                {
                    var connIds = _slotsMapping[slot].ToList();
                    connIds.Remove(connectionId);
                    _slotsMapping[slot] = connIds.ToArray();
                    _logger.LogDebug("{ConnectionId} unmapped from slot {Slot}", connectionId, slot);
                    return Task.CompletedTask;
                }
            }
            return Task.CompletedTask;
        }
    }
}
