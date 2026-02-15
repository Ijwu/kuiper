using System.Text.Json;
using System.Text.RegularExpressions;

using kbo.bigrocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.DataStorage.Plugins
{
    public class DataStorageSlotDataPlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly Regex _slotNumberRegex = new Regex("_read_slot_data_(\\d+)");

        public DataStorageSlotDataPlugin(ILogger<DataStorageSlotDataPlugin> logger, IConnectionManager connectionManager, MultiData multiData)
            : base(logger, connectionManager)
        {
            _multiData = multiData;
        }

        protected override void RegisterHandlers()
        {
            Handle<Get>(HandleGetAsync);
        }

        private async Task HandleGetAsync(Get getPacket, string connectionId)
        {
            Dictionary<string, object> responseData = [];
            foreach (string key in getPacket.Keys)
            {
                Match match = _slotNumberRegex.Match(key);
                if (!match.Success)
                {
                    continue;
                }

                string slotValue = match.Groups[1].Value;
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

            Retrieved responsePacket = new(responseData.ToDictionary(kvp => kvp.Key, kvp => JsonSerializer.SerializeToNode(kvp.Value)!));
            await SendToConnectionAsync(connectionId, responsePacket);
        }
    }
}
