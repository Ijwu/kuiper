using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class LocationScoutsPlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly IHintService _hintService;
        private readonly IServerAnnouncementService _announcementService;

        public LocationScoutsPlugin(ILogger<LocationScoutsPlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData, IHintService hintService, IServerAnnouncementService announcementService)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
        }

        protected override void RegisterHandlers()
        {
            Handle<LocationScouts>(HandleLocationScoutsAsync);
        }

        private async Task HandleLocationScoutsAsync(LocationScouts locationScoutsPacket, string connectionId)
        {
            Logger.LogDebug("Handling LocationScoutsPacket for connection {ConnectionId}", connectionId);

            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
                return;

            var allLocationsForSlot = _multiData.Locations[slotId];

            var scouts = locationScoutsPacket.Locations
                .Where(loc => allLocationsForSlot.ContainsKey((int)loc))
                .ToDictionary(loc => (int)loc, loc => allLocationsForSlot[(int)loc])
                .Select(kvp => new NetworkItem(kvp.Value[0], kvp.Key, (int)kvp.Value[1], (NetworkItemFlags)kvp.Value[2]))
                .ToArray();

            if (scouts.Length > 0)
            {
                var responsePacket = new LocationInfo(scouts);
                await SendToConnectionAsync(connectionId, responsePacket);
                Logger.LogInformation("Sent {Length} scouted locations to connection {ConnectionId}", scouts.Length, connectionId);

                await CreateHintsIfRequestedAsync(locationScoutsPacket.CreateAsHint, slotId, scouts);
            }
            else
            {
                Logger.LogDebug("No valid locations in LocationScoutsPacket from connection {ConnectionId}", connectionId);
            }
        }

        private async Task CreateHintsIfRequestedAsync(long createAsHint, long findingPlayer, NetworkItem[] scoutedItems)
        {
            if (createAsHint == 0)
                return;

            int hintsCreated = 0;

            foreach (var item in scoutedItems)
            {
                if (createAsHint == 2)
                {
                    bool exists = await _hintService.HintExistsForLocationAsync(item.Location);
                    if (exists)
                        continue;
                }

                var hint = new Hint(
                    receivingPlayer: item.Player,
                    findingPlayer: findingPlayer,
                    location: item.Location,
                    item: item.Item,
                    found: false,
                    entrance: string.Empty,
                    itemFlags: item.Flags,
                    status: HintStatus.Unspecified
                );

                await _hintService.AddHintAsync(item.Player, hint);
                await _announcementService.AnnounceHintAsync(item.Player, findingPlayer, item.Item, item.Location, item.Flags);
                hintsCreated++;
            }

            if (hintsCreated > 0)
            {
                Logger.LogInformation("Created {Count} hints from location scouts (CreateAsHint={Mode})", hintsCreated, createAsHint);
            }
        }
    }
}
