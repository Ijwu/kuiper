using System.Text.Json;
using System.Text.RegularExpressions;

using kbo.bigrocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class DataStorageSlotDataPlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly Regex _slotNumberRegex = new Regex("_read_slot_data_(\\d+)");

        public DataStorageSlotDataPlugin(ILogger<DataStorageSlotDataPlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        protected override void RegisterHandlers()
        {
            Handle<Get>(HandleGetAsync);
        }

        private async Task HandleGetAsync(Get getPacket, string connectionId)
        {
            Dictionary<string, object> responseData = new();
            foreach (var key in getPacket.Keys)
            {
                if (!key.StartsWith("_read_slot_data")) // reject keys unhandled by this plugin
                    continue;

                Logger.LogDebug("Handling GetPacket for slot data key '{Key}' from connection {ConnectionId}", key, connectionId);

                var slotValue = _slotNumberRegex.Match(key).Groups[1].Value;

                if (!int.TryParse(slotValue, out var slotId))
                {
                    Logger.LogDebug("Invalid slot ID '{SlotValue}' extracted from key '{Key}' requested by connection {ConnectionId}", slotValue, key, connectionId);
                    continue;
                }

                if (_multiData.SlotData.TryGetValue(slotId, out var data))
                {
                    responseData.Add(key, data);
                    Logger.LogDebug("Found slot data for key `{Key}` requested by connection {ConnectionId}", key, connectionId);
                }
                else
                {
                    Logger.LogDebug("No slot data found for key '{Key}' requested by connection {ConnectionId}", key, connectionId);
                }
            }

            if (responseData.Count > 0)
            {
                var responsePacket = new Retrieved(responseData.ToDictionary(kvp => kvp.Key, kvp => JsonSerializer.SerializeToNode(kvp.Value)));
                await SendToConnectionAsync(connectionId, responsePacket);
            }
        }
    }
}
