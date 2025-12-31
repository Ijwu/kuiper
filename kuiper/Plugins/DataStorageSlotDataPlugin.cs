using System.Text.Json;
using System.Text.RegularExpressions;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class DataStorageSlotDataPlugin : IPlugin
    {
        private readonly ILogger<DataStorageSlotDataPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;
        private readonly Regex _slotNumberRegex = new Regex("_read_slot_data_(\\d+)");

        public DataStorageSlotDataPlugin(ILogger<DataStorageSlotDataPlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _multiData = multiData;
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not Get getPacket)
                return;

            Dictionary<string, object> responseData = new();
            foreach (var key in getPacket.Keys)
            {
                if (!key.StartsWith("_read_slot_data")) // reject keys unhandled by this plugin
                    continue;

                _logger.LogDebug("Handling GetPacket for slot data key '{Key}' from connection {ConnectionId}", key, connectionId);

                var slotValue = _slotNumberRegex.Match(key).Groups[1].Value;

                if (!int.TryParse(slotValue, out var slotId))
                {
                    _logger.LogDebug("Invalid slot ID '{SlotValue}' extracted from key '{Key}' requested by connection {ConnectionId}", slotValue, key, connectionId);
                    continue;
                }

                if (_multiData.SlotData.TryGetValue(slotId, out var data))
                {
                    responseData.Add(key, data);
                    _logger.LogDebug("Found slot data for key `{Key}` requested by connection {ConnectionId}", key, connectionId);
                }
                else
                {
                    _logger.LogDebug("No slot data found for key '{Key}' requested by connection {ConnectionId}", key, connectionId);
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
