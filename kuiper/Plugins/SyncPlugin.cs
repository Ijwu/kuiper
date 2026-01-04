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
        private readonly IReceivedItemService _receivedItems;
        private readonly MultiData _multiData;

        public SyncPlugin(ILogger<SyncPlugin> logger, WebSocketConnectionManager connectionManager, IReceivedItemService receivedItems, MultiData multiData)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _receivedItems = receivedItems;
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

            var receivedItems = await _receivedItems.GetReceivedItemsAsync(slotId);
            foreach (var (item, sendingSlot) in receivedItems)
            {
                item.Player = sendingSlot;
                items.Add(item);
            }

            var responsePacket = new ReceivedItems(0, items.ToArray());

            await _connectionManager.SendJsonToConnectionAsync(connectionId, new[] { responsePacket });
        }
    }
}
