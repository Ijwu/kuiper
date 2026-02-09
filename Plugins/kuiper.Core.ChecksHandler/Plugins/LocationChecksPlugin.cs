using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.ChecksHandler.Plugins
{
    public class LocationChecksPlugin : BasePlugin
    {
        private readonly ILocationCheckService _locationChecks;
        private readonly IHintPointsService _hintPoints;
        private readonly IServerAnnouncementService _announcementService;
        private readonly IReceivedItemService _receivedItems;
        private readonly MultiData _multiData;

        public LocationChecksPlugin(ILogger<LocationChecksPlugin> logger,
                                    ILocationCheckService locationChecks,
                                    IConnectionManager connectionManager,
                                    IHintPointsService hintPoints,
                                    IServerAnnouncementService announcementService,
                                    IReceivedItemService receivedItems,
                                    MultiData multiData)
            : base(logger, connectionManager)
        {
            _locationChecks = locationChecks;
            _hintPoints = hintPoints;
            _announcementService = announcementService;
            _receivedItems = receivedItems;
            _multiData = multiData;
        }

        protected override void RegisterHandlers()
        {
            Handle<LocationChecks>(HandleLocationChecksAsync);
        }

        private async Task HandleLocationChecksAsync(LocationChecks locPacket, string connectionId)
        {
            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
            {
                Logger.LogDebug("Unmapped connection ({ConnectionId}) attempted to send location checks packet.", connectionId);
                return;
            }

            // Determine which locations are new (not already recorded)
            var existing = (await _locationChecks.GetChecksAsync(slotId)).ToHashSet();
            long[] newLocations = locPacket.Locations?.Where(l => !existing.Contains(l)).ToArray() ?? [];


            List<NetworkItem> receivedItemsToSend = [];
            if (newLocations.Length > 0)
            {
                foreach (var loc in newLocations)
                {
                    var item = await _locationChecks.AddCheckAsync(slotId, loc);
                    if (item != null)
                    {
                        receivedItemsToSend.Add(item);                            
                    }
                }

                // Reward hint points for new checks (Default: 1 point per check)
                int pointsPerCheck = _multiData.ServerOptions.TryGetValue("location_check_points", out var lcp) ? Convert.ToInt32(lcp) : 1;
                await _hintPoints.AddHintPointsAsync(slotId, pointsPerCheck * newLocations.Length);
                Logger.LogDebug("Recorded {Count} new location checks for slot {Slot} from connection {ConnectionId}", newLocations.Length, slotId, connectionId);

                // Send RoomUpdate to caller with updated hint points and newly checked locations
                var updatedHintPoints = await _hintPoints.GetHintPointsAsync(slotId);
                var roomUpdate = new RoomUpdate(
                    hintPoints: updatedHintPoints,
                    players: null,
                    checkedLocations: newLocations.Select(l => (long)l).ToArray(),
                    hintCost: null,
                    locationCheckPoints: null,
                    permissions: null
                );
                await SendToConnectionAsync(connectionId, roomUpdate);
            }
            else
            {
                Logger.LogDebug("No new locations in LocationChecks from connection {ConnectionId}", connectionId);
            }

            Dictionary<long, List<NetworkItem>> itemsByPlayer = new();
            foreach (NetworkItem item in receivedItemsToSend)
            {
                if (!itemsByPlayer.ContainsKey(item.Player))
                {
                    itemsByPlayer[item.Player] = [ item ];
                }
                else
                {
                    itemsByPlayer[item.Player] = [..itemsByPlayer[item.Player], item];
                }
            }

            // Send back items grouped by player
            foreach (KeyValuePair<long, List<NetworkItem>> player in itemsByPlayer)
            {
                int receivedSoFar = (await _receivedItems.GetReceivedItemsAsync(player.Key)).Count();
                int index = Math.Max(0, receivedSoFar - player.Value.Count);

                List<NetworkItem> itemListForPlayer = [..player.Value.Select(x => new NetworkItem(x.Item, x.Location, x.Player, x.Flags))];
                itemListForPlayer.ForEach(x => x.Player = slotId);
                ReceivedItems responsePacket = new(index, itemListForPlayer.ToArray());

                await SendToSlotAsync(player.Key, responsePacket);

                // Announce item sends
                foreach (var item in player.Value)
                {
                    await _announcementService.AnnounceItemSentAsync(slotId, item.Player, GetItemName(slotId, item.Item), item.Item, item.Location);
                }
            }
        }

        private string GetItemName(long sourceSlotId, long itemId)
        {
            try
            {
                if (_multiData.SlotInfo.TryGetValue((int)sourceSlotId, out var slot))
                {
                    var game = slot.Game;
                    if (_multiData.DataPackage.TryGetValue(game, out var package))
                    {
                        var name = package.ItemNameToId.FirstOrDefault(kvp => kvp.Value == itemId).Key;
                        return string.IsNullOrWhiteSpace(name) ? itemId.ToString() : name;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Failed to resolve item name for {ItemId}; using id.", itemId);
            }
            return itemId.ToString();
        }
    }
}
