using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class SyncPlugin : BasePlugin
    {
        private readonly IReceivedItemService _receivedItems;
        private readonly MultiData _multiData;

        public SyncPlugin(ILogger<SyncPlugin> logger, WebSocketConnectionManager connectionManager, IReceivedItemService receivedItems, MultiData multiData)
            : base(logger, connectionManager)
        {
            _receivedItems = receivedItems ?? throw new ArgumentNullException(nameof(receivedItems));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        protected override void RegisterHandlers()
        {
            Handle<Sync>(HandleSyncAsync);
        }

        private async Task HandleSyncAsync(Sync packet, string connectionId)
        {
            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
                return;

            List<NetworkItem> items = new();

            var receivedItems = await _receivedItems.GetReceivedItemsAsync(slotId);
            foreach (var (item, sendingSlot) in receivedItems)
            {
                item.Player = sendingSlot;
                items.Add(item);
            }

            var responsePacket = new ReceivedItems(0, items.ToArray());
            await SendToConnectionAsync(connectionId, responsePacket);
        }
    }
}
