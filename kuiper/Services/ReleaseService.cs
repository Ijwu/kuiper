using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services.Abstract;

namespace kuiper.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ILogger<ReleaseService> _logger;
        private readonly ILocationCheckService _locationChecks;
        private readonly MultiData _multiData;
        private readonly IServerAnnouncementService _announcementService;
        private readonly WebSocketConnectionManager _connectionManager;

        public ReleaseService(
            ILogger<ReleaseService> logger,
            ILocationCheckService locationChecks,
            MultiData multiData,
            IServerAnnouncementService announcementService,
            WebSocketConnectionManager connectionManager)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locationChecks = locationChecks ?? throw new ArgumentNullException(nameof(locationChecks));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }

        public async Task ReleaseRemainingItemsAsync(long slotId)
        {
            if (!_multiData.Locations.TryGetValue(slotId, out var slotLocations) || slotLocations.Count == 0)
            {
                _logger.LogDebug("No location data found for slot {Slot}; nothing to release.", slotId);
                return;
            }

            var recordedChecks = (await _locationChecks.GetChecksAsync(slotId)).ToHashSet();
            var remainingLocations = slotLocations.Keys.Where(loc => !recordedChecks.Contains(loc)).ToArray();
            if (remainingLocations.Length == 0)
            {
                _logger.LogInformation("Slot {Slot} has no remaining items to release.", slotId);
                return;
            }

            var releasedItems = new List<NetworkItem>();
            foreach (var locationId in remainingLocations)
            {
                var item = await _locationChecks.AddCheckAsync(slotId, locationId);
                if (item != null)
                {
                    releasedItems.Add(item);
                }
            }

            if (releasedItems.Count == 0)
            {
                _logger.LogInformation("Slot {Slot} had no releasable items after reaching goal.", slotId);
                return;
            }

            foreach (var group in releasedItems.GroupBy(item => item.Player))
            {
                var totalChecks = await _locationChecks.GetChecksAsync(group.Key);
                var packet = new ReceivedItems(totalChecks.Count(), group.ToArray());

                await SendToSlotAsync(group.Key, packet);

                foreach (var item in group)
                {
                    await _announcementService.AnnounceItemSentAsync(slotId, item.Player, GetItemName(slotId, item.Item), item.Item, item.Location);
                }

                _logger.LogInformation("Released {Count} remaining item(s) from slot {SourceSlot} to slot {TargetSlot}.", group.Count(), slotId, group.Key);
            }
        }

        private async Task SendToSlotAsync(long slotId, params Packet[] packets)
        {
            var connectionIds = await _connectionManager.GetConnectionIdsForSlotAsync(slotId);
            foreach (var connId in connectionIds)
            {
                await _connectionManager.SendJsonToConnectionAsync(connId, packets);
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
