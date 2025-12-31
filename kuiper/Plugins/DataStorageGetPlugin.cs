using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class DataStorageGetPlugin : IPlugin
    {
        private readonly ILogger<DataStorageGetPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IStorageService _storage;

        public DataStorageGetPlugin(ILogger<DataStorageGetPlugin> logger, WebSocketConnectionManager connectionManager, IStorageService storage)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _storage = storage;
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not Get getPacket)
                return;

            Dictionary<string, object> responseData = new();
            foreach (var key in getPacket.Keys)
            {
                if (key.StartsWith("_")) // handle internal keys with other plugins
                    continue;

                var data = await _storage.LoadAsync<object>(key);
                if (data != null)
                {
                    responseData.Add(key, data);
                }
                else
                {
                    _logger.LogDebug("No data found for key '{Key}' requested by connection {ConnectionId}", key, connectionId);
                }
            }

            if (responseData.Count > 0)
            {
                var responsePacket = new Retrieved(responseData.ToDictionary(kvp => kvp.Key, kvp => JsonSerializer.SerializeToNode(kvp.Value)));

                await _connectionManager.SendJsonToConnectionAsync(connectionId, new[] { responsePacket });
            }
        }
    }
}
