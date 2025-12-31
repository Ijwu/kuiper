using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class LocationScoutsPlugin : IPlugin
    {
        private readonly ILogger<LocationScoutsPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;

        public LocationScoutsPlugin(ILogger<LocationScoutsPlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _multiData = multiData;
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not LocationScouts locationScoutsPacket)
                return;

            _logger.LogDebug("Handling LocationScoutsPacket for connection {ConnectionId}", connectionId);

            long? slotId = await _connectionManager.GetSlotForConnectionAsync(connectionId);

            if (slotId == null)
            {
                _logger.LogDebug("Received LocationScoutsPacket from connection {ConnectionId} with no mapped slot; ignoring.", connectionId);
                return;
            }

            var allLocationsForSlot = _multiData.Locations[slotId.Value];

            var scouts = locationScoutsPacket.Locations
                .Where(loc => allLocationsForSlot.ContainsKey((int)loc))
                .ToDictionary(loc => (int)loc, loc => allLocationsForSlot[(int)loc])
                .Select(kvp => new NetworkItem(kvp.Value[0], kvp.Key, (int)kvp.Value[1], (NetworkItemFlags)kvp.Value[2]))
                .ToArray();

            if (scouts.Length > 0)
            {
                var responsePacket = new LocationInfo(scouts);
                await _connectionManager.SendJsonToConnectionAsync(connectionId, new[] { responsePacket });
                _logger.LogInformation("Sent {Length} scouted locations to connection {ConnectionId}", scouts.Length, connectionId);
            }
            else
            {
                _logger.LogDebug("No valid locations in LocationScoutsPacket from connection {ConnectionId}", connectionId);
            }
        }
    }
}
