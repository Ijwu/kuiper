using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ILogger<ReleaseService> _logger;
        private readonly ILocationCheckService _locationChecks;
        private readonly MultiData _multiData;
        private readonly IServerAnnouncementService _announcementService;
        private readonly IConnectionManager _connectionManager;

        public ReleaseService(
            ILogger<ReleaseService> logger,
            ILocationCheckService locationChecks,
            MultiData multiData,
            IServerAnnouncementService announcementService,
            IConnectionManager connectionManager)
        {
            _logger = logger;
            _locationChecks = locationChecks;
            _multiData = multiData;
            _announcementService = announcementService;
            _connectionManager = connectionManager;
        }

        public async Task ReleaseRemainingItemsAsync(long slotId)
        {
            if (!_multiData.Locations.TryGetValue(slotId, out var slotLocations) || slotLocations.Count == 0)
            {
                _logger.LogDebug("No location data found for slot {Slot}; nothing to release.", slotId);
                return;
            }

            HashSet<long> recordedChecks = (await _locationChecks.GetChecksAsync(slotId)).ToHashSet();
            long[] remainingLocations = slotLocations.Keys.Where(loc => !recordedChecks.Contains(loc)).ToArray();
            if (remainingLocations.Length == 0)
            {
                _logger.LogDebug("Slot {Slot} has no remaining items to release.", slotId);
                return;
            }

            List<NetworkItem> releasedItems = [];
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
                _logger.LogDebug("Slot {Slot} had no releasable items after reaching goal.", slotId);
                return;
            }

            foreach (var group in releasedItems.GroupBy(item => item.Player))
            {
                IEnumerable<long> totalChecks = await _locationChecks.GetChecksAsync(group.Key);
                ReceivedItems packet = new(totalChecks.Count(), group.ToArray());

                await SendToSlotAsync(group.Key, packet);

                foreach (NetworkItem item in group)
                {
                    await _announcementService.AnnounceItemSentAsync(slotId, item.Player, GetItemName(slotId, item.Item), item.Item, item.Location);
                }

                _logger.LogDebug("Released {Count} remaining item(s) from slot {SourceSlot} to slot {TargetSlot}.", group.Count(), slotId, group.Key);
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
            if (_multiData.SlotInfo.TryGetValue((int)sourceSlotId, out var slot))
            {
                var game = slot.Game;
                if (_multiData.DataPackage.TryGetValue(game, out var package))
                {
                    var name = package.ItemNameToId.FirstOrDefault(kvp => kvp.Value == itemId).Key;
                    return string.IsNullOrWhiteSpace(name) ? itemId.ToString() : name;
                }
            }

            return itemId.ToString();
        }
    }
}
