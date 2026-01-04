using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class LocationChecksPlugin : IPlugin
    {
        private readonly ILogger<LocationChecksPlugin> _logger;
        private readonly ILocationCheckService _locationChecks;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IHintPointsService _hintPoints;
        private readonly IServerAnnouncementService _announcementService;
        private readonly IReceivedItemService _receivedItems;
        private readonly MultiData _multiData;

        public LocationChecksPlugin(ILogger<LocationChecksPlugin> logger, ILocationCheckService locationChecks, WebSocketConnectionManager connectionManager, IHintPointsService hintPoints, IServerAnnouncementService announcementService, IReceivedItemService receivedItems, MultiData multiData)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locationChecks = locationChecks ?? throw new ArgumentNullException(nameof(locationChecks));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _hintPoints = hintPoints ?? throw new ArgumentNullException(nameof(hintPoints));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _receivedItems = receivedItems ?? throw new ArgumentNullException(nameof(receivedItems));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not LocationChecks locPacket)
                return;

            try
            {
                var slotIdNullable = await _connectionManager.GetSlotForConnectionAsync(connectionId);
                if (!slotIdNullable.HasValue)
                {
                    _logger.LogDebug("Received LocationChecks from connection {ConnectionId} with no mapped slot; ignoring.", connectionId);
                    return;
                }

                var slotId = slotIdNullable.Value;

                // Determine which locations are new (not already recorded)
                var existing = (await _locationChecks.GetChecksAsync(slotId)).ToHashSet();
                var newLocations = locPacket.Locations?.Where(l => !existing.Contains((int)l)).Select(l => (int)l).ToArray() ?? Array.Empty<int>();


                List<NetworkItem> items = new();
                if (newLocations.Length > 0)
                {
                    foreach (var loc in newLocations)
                    {
                        var item = await _locationChecks.AddCheckAsync(slotId, loc);
                        if (item != null)
                        {
                            items.Add(item);                            
                        }
                    }

                    // TODO: reward correct hint points
                    // Reward hint points for new checks (example: 1 point per check)
                    await _hintPoints.AddHintPointsAsync(slotId, newLocations.Length);
                    _logger.LogInformation("Recorded {Count} new location checks for slot {Slot} from connection {ConnectionId}", newLocations.Length, slotId, connectionId);

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
                    await _connectionManager.SendJsonToConnectionAsync(connectionId, new[] { roomUpdate });
                }
                else
                {
                    _logger.LogDebug("No new locations in LocationChecks from connection {ConnectionId}", connectionId);
                }

                Dictionary<long, List<NetworkItem>> itemsByPlayer = new();
                foreach (NetworkItem item in items)
                {
                    if (!itemsByPlayer.ContainsKey(item.Player))
                    {
                        itemsByPlayer[item.Player] = [ item ];
                    }
                    else
                    {
                        var list = itemsByPlayer[item.Player];
                        list.Add(item);
                    }
                }

                // Send back items grouped by player
                foreach (var player in itemsByPlayer)
                {
                    var receivedSoFar = (await _receivedItems.GetReceivedItemsAsync(player.Key)).Count();
                    var index = Math.Max(0, receivedSoFar - player.Value.Count);

                    var responsePacket = new ReceivedItems(index, player.Value.ToArray());

                    var targetConnectionIds = await _connectionManager.GetConnectionIdsForSlotAsync(player.Key);
                    if (targetConnectionIds.Count == 0)
                    {
                        _logger.LogDebug("No active connections mapped to slot {Slot}; skipping ReceivedItems broadcast.", player.Key);
                        continue;
                    }

                    foreach (var connection in targetConnectionIds)
                    {
                        await _connectionManager.SendJsonToConnectionAsync(connection, new[] { responsePacket });
                    }

                    // Announce item sends
                    foreach (var item in player.Value)
                    {
                        await _announcementService.AnnounceItemSentAsync(slotId, item.Player, GetItemName(slotId, item.Item), item.Item, item.Location);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling LocationChecks packet from {ConnectionId}", connectionId);
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
                _logger.LogDebug(ex, "Failed to resolve item name for {ItemId}; using id.", itemId);
            }
            return itemId.ToString();
        }
    }
}
