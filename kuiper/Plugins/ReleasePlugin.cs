using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class ReleasePlugin : BasePlugin
    {
        private readonly ILocationCheckService _locationChecks;
        private readonly MultiData _multiData;
        private readonly IServerAnnouncementService _announcementService;

        public ReleasePlugin(
            ILogger<ReleasePlugin> logger,
            WebSocketConnectionManager connectionManager,
            ILocationCheckService locationChecks,
            MultiData multiData,
            IServerAnnouncementService announcementService)
            : base(logger, connectionManager)
        {
            _locationChecks = locationChecks ?? throw new ArgumentNullException(nameof(locationChecks));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
        }

        protected override void RegisterHandlers()
        {
            Handle<StatusUpdate>(HandleStatusUpdateAsync);
        }

        private async Task HandleStatusUpdateAsync(StatusUpdate statusPacket, string connectionId)
        {
            if (statusPacket.Status != ClientStatus.Goal)
                return;

            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
                return;

            await _announcementService.AnnounceGoalReachedAsync(slotId, GetPlayerName(slotId));
            await ReleaseRemainingItemsAsync(slotId);
        }

        private async Task ReleaseRemainingItemsAsync(long slotId)
        {
            if (!_multiData.Locations.TryGetValue(slotId, out var slotLocations) || slotLocations.Count == 0)
            {
                Logger.LogDebug("No location data found for slot {Slot}; nothing to release.", slotId);
                return;
            }

            var recordedChecks = (await _locationChecks.GetChecksAsync(slotId)).ToHashSet();
            var remainingLocations = slotLocations.Keys.Where(loc => !recordedChecks.Contains(loc)).ToArray();
            if (remainingLocations.Length == 0)
            {
                Logger.LogInformation("Slot {Slot} has no remaining items to release.", slotId);
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
                Logger.LogInformation("Slot {Slot} had no releasable items after reaching goal.", slotId);
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

                Logger.LogInformation("Released {Count} remaining item(s) from slot {SourceSlot} to slot {TargetSlot}.", group.Count(), slotId, group.Key);
            }
        }

        private string GetPlayerName(long slotId)
        {
            if (_multiData.SlotInfo.TryGetValue((int)slotId, out var info))
                return info.Name;
            return $"Player {slotId}";
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
