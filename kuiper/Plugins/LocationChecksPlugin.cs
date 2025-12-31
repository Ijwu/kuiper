using kbo.bigrocks;
using kbo.littlerocks;

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

        public LocationChecksPlugin(ILogger<LocationChecksPlugin> logger, ILocationCheckService locationChecks, WebSocketConnectionManager connectionManager, IHintPointsService hintPoints)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _locationChecks = locationChecks ?? throw new ArgumentNullException(nameof(locationChecks));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _hintPoints = hintPoints ?? throw new ArgumentNullException(nameof(hintPoints));
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
                    var totalChecks = await _locationChecks.GetChecksAsync(player.Key);

                    var responsePacket = new ReceivedItems(totalChecks.Count(), items.ToArray());

                    var targetConnectionIds = await _connectionManager.GetConnectionIdsForSlotAsync(player.Key);
                    if (targetConnectionIds.Count == 0)
                    {
                        _logger.LogDebug("No active connections mapped to slot {Slot}; skipping ReceivedItems broadcast.", player.Key);
                        continue;
                    }

                    foreach (var targetConnectionId in targetConnectionIds)
                    {
                        await _connectionManager.SendJsonToConnectionAsync(targetConnectionId, new[] { responsePacket });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling LocationChecks packet from {ConnectionId}", connectionId);
            }
        }
    }
}
