using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;

namespace kuiper.Core.Services
{
    public class InMemoryNotifyingStorageService : INotifyingStorageService
    {
        private static readonly object NullMarker = new();

        private readonly ConcurrentDictionary<string, object> _store = new();
        private readonly IConnectionManager _connectionManager;

        public InMemoryNotifyingStorageService(IConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public async Task SaveAsync<T>(string key, T value, long settingSlot)
        {
            object? originalValue = null;
            if (_store.ContainsKey(key))
            {
                originalValue = _store[key];
            }

            // store the value directly; use a marker for null because ConcurrentDictionary disallows null values
            _store[key] = value is null ? NullMarker : value;

            await NotifyConnectionsWaitingForSetReply(key, value, originalValue, settingSlot);
        }

        public Task<T?> LoadAsync<T>(string key)
        {
            if (_store.TryGetValue(key, out var obj))
            {
                if (ReferenceEquals(obj, NullMarker))
                    return Task.FromResult<T?>(default);

                if (obj is T t)
                    return Task.FromResult(t)!;

                // stored value is not assignable to T
                return Task.FromResult<T?>(default);
            }

            return Task.FromResult<T?>(default);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_store.ContainsKey(key));
        }

        public async Task DeleteAsync(string key, long settingSlot)
        {
            if (!_store.TryGetValue(key, out var originalValue))
            {
                return;
            }

            _store.TryRemove(key, out _);

            await NotifyConnectionsWaitingForSetReply(key, null, originalValue, settingSlot);
        }

        public Task<IEnumerable<string>> ListKeysAsync()
        {
            return Task.FromResult(_store.Keys.AsEnumerable());
        }

        private async Task NotifyConnectionsWaitingForSetReply(string key, object? value, object? originalValue, long slotThatSetKey)
        {
            IEnumerable<string> keys = _store.Keys.Where(x => x.StartsWith(StorageKeys.SetNotifyPrefix));
            List<string> connectionsToNotify = [];
            foreach (var setNotifyKey in keys)
            {
                string connectionId = setNotifyKey.Substring(StorageKeys.SetNotifyPrefix.Length);
                string[]? watchedKeysForConnection = _store[setNotifyKey] as string[];

                if (watchedKeysForConnection == null)
                {
                    continue;
                }

                if (watchedKeysForConnection.Any(x => x == key))
                {
                    connectionsToNotify.Add(connectionId);
                }
            }

            JsonNode? valueNode = value as JsonNode;
            if (valueNode == null)
            {
                valueNode = JsonSerializer.SerializeToNode(value);
            }

            JsonNode? originalNode = originalValue as JsonNode;
            if (originalNode == null)
            {
                originalNode = JsonSerializer.SerializeToNode(originalValue);
            }


            SetReply reply = new(key, valueNode!, originalNode!, slotThatSetKey);

            foreach (var connection in connectionsToNotify)
            {
                await _connectionManager.SendJsonToConnectionAsync<Packet[]>(connection, [reply]);
            }
        }
    }
}
