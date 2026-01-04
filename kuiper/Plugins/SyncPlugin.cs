using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class SyncPlugin : IPlugin
    {
        private readonly ILogger<SyncPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly ILocationCheckService _locationChecks;
        private readonly MultiData _multiData;

        public SyncPlugin(ILogger<SyncPlugin> logger, WebSocketConnectionManager connectionManager, ILocationCheckService locationChecks, MultiData multiData)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _locationChecks = locationChecks;
            _multiData = multiData;
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not Sync syncPacket)
                return;

            var slotIdNullable = await _connectionManager.GetSlotForConnectionAsync(connectionId);
            if (!slotIdNullable.HasValue)
            {
                _logger.LogDebug("Received SyncPacket from connection {ConnectionId} with no mapped slot; ignoring.", connectionId);
                return;
            }
            var slotId = slotIdNullable.Value;

            List<NetworkItem> items = new();

            foreach (var slotLocations in _multiData.Locations)
            {
                var sourceSlot = slotLocations.Key;
                var checks = await _locationChecks.GetChecksAsync(sourceSlot);

                foreach (var loc in checks)
                {
                    if (!slotLocations.Value.TryGetValue(loc, out var itemData))
                        continue;

                    if (itemData[1] != slotId)
                        continue;

                    var item = new NetworkItem(itemData[0], loc, sourceSlot, (NetworkItemFlags)itemData[2]);
                    items.Add(item);
                }
            }

            var responsePacket = new ReceivedItems(0, items.ToArray());

            await _connectionManager.SendJsonToConnectionAsync(connectionId, new[] { responsePacket });
        }
    }
}
