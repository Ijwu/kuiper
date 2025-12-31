using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class ReleasePlugin : IPlugin
    {
        private readonly ILogger<ReleasePlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly ILocationCheckService _locationChecks;
        private readonly MultiData _multiData;

        public ReleasePlugin(
            ILogger<ReleasePlugin> logger,
            WebSocketConnectionManager connectionManager,
            ILocationCheckService locationChecks,
            MultiData multiData)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _locationChecks = locationChecks ?? throw new ArgumentNullException(nameof(locationChecks));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not StatusUpdate statusPacket)
                return;

            if (statusPacket.Status != ClientStatus.Goal)
                return;

            var slotId = await _connectionManager.GetSlotForConnectionAsync(connectionId);
            if (!slotId.HasValue)
            {
                _logger.LogDebug("Received ClientGoal status from {ConnectionId} without a mapped slot; skipping remaining release.", connectionId);
                return;
            }

            await ReleaseRemainingItemsAsync(slotId.Value);
        }

        private async Task ReleaseRemainingItemsAsync(long slotId)
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

                var targetConnectionIds = await _connectionManager.GetConnectionIdsForSlotAsync(group.Key);
                if (targetConnectionIds.Count == 0)
                {
                    _logger.LogInformation("Could not deliver {Count} remaining item(s) from slot {SourceSlot} to slot {TargetSlot} because the target is offline.", group.Count(), slotId, group.Key);
                    continue;
                }

                var totalChecks = await _locationChecks.GetChecksAsync(group.Key);

                var packet = new ReceivedItems(totalChecks.Count(), group.ToArray());

                foreach (var connection in targetConnectionIds)
                {
                    await _connectionManager.SendJsonToConnectionAsync(connection, new[] { packet });
                }

                _logger.LogInformation("Released {Count} remaining item(s) from slot {SourceSlot} to slot {TargetSlot}.", group.Count(), slotId, group.Key);
            }
        }
    }
}
