using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class CreateHintsPlugin : IPlugin
    {
        private readonly ILogger<CreateHintsPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;
        private readonly IHintService _hintService;

        public CreateHintsPlugin(ILogger<CreateHintsPlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData, IHintService hintService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not CreateHints createHints || createHints.Locations is null || createHints.Locations.Length == 0)
                return;

            var slotId = await _connectionManager.GetSlotForConnectionAsync(connectionId);
            if (!slotId.HasValue)
            {
                _logger.LogDebug("Received CreateHints from {ConnectionId} with no mapped slot; ignoring.", connectionId);
                return;
            }

            var targetSlotId = createHints.Player != 0 ? createHints.Player : slotId.Value;
            if (!_multiData.Locations.TryGetValue(targetSlotId, out var slotLocations))
            {
                _logger.LogDebug("No location data found for slot {Slot} while handling CreateHints.", targetSlotId);
                return;
            }
            var hintStatus = createHints.Status;

            foreach (var loc in createHints.Locations)
            {
                if (!slotLocations.TryGetValue(loc, out var data) || data.Length < 3)
                {
                    _logger.LogDebug("Location {Location} not found for slot {Slot} while handling CreateHints.", loc, targetSlotId);
                    continue;
                }

                var itemId = data[0];
                var receivingPlayer = data[1];
                var itemFlags = (NetworkItemFlags)data[2];

                var hint = new Hint(receivingPlayer, targetSlotId, loc, itemId, found: false, entrance: string.Empty, itemFlags: itemFlags);
                await _hintService.AddHintAsync(targetSlotId, hint, hintStatus);
            }
        }
    }
}
