using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Checks.Plugins
{
    public class SyncPlugin : BasePlugin
    {
        private readonly IReceivedItemService _receivedItems;

        public SyncPlugin(ILogger<SyncPlugin> logger, IConnectionManager connectionManager, IReceivedItemService receivedItems)
            : base(logger, connectionManager)
        {
            _receivedItems = receivedItems;
        }

        protected override void RegisterHandlers()
        {
            Handle<Sync>(HandleSyncAsync);
        }

        private async Task HandleSyncAsync(Sync packet, string connectionId)
        {
            (bool success, long slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
            {
                return;
            }

            List<NetworkItem> items = [];

            var receivedItems = await _receivedItems.GetReceivedItemsAsync(slotId);
            foreach ((NetworkItem item, long sendingSlot) in receivedItems)
            {
                item.Player = sendingSlot;
                items.Add(item);
            }

            ReceivedItems responsePacket = new(0, items.ToArray());
            await SendToConnectionAsync(connectionId, responsePacket);
        }
    }
}
