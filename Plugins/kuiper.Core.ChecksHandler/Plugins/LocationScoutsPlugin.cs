using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.ChecksHandler.Plugins
{
    public class LocationScoutsPlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly IHintService _hintService;
        private readonly IServerAnnouncementService _announcementService;

        public LocationScoutsPlugin(ILogger<LocationScoutsPlugin> logger,
                                    IConnectionManager connectionManager,
                                    MultiData multiData,
                                    IHintService hintService,
                                    IServerAnnouncementService announcementService)
            : base(logger, connectionManager)
        {
            _multiData = multiData;
            _hintService = hintService;
            _announcementService = announcementService;
        }

        protected override void RegisterHandlers()
        {
            Handle<LocationScouts>(HandleLocationScoutsAsync);
        }

        private async Task HandleLocationScoutsAsync(LocationScouts locationScoutsPacket, string connectionId)
        {
            Logger.LogDebug("Handling LocationScouts for connection {ConnectionId}", connectionId);

            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
            {
                Logger.LogDebug("Could not map connection to slot while handling LocationScouts for connection {ConnectionId}", connectionId);
                return;
            }

            Dictionary<long, long[]> allLocationsForSlot = _multiData.Locations[slotId];

            NetworkItem[] scouts = locationScoutsPacket.Locations
                .Where(allLocationsForSlot.ContainsKey)
                .Select(loc => new NetworkItem(allLocationsForSlot[loc][0], loc, allLocationsForSlot[loc][1], (NetworkItemFlags)allLocationsForSlot[loc][2]))
                .ToArray();

            if (scouts.Length > 0)
            {
                LocationInfo responsePacket = new(scouts);
                await SendToConnectionAsync(connectionId, responsePacket);
                Logger.LogDebug("Sent {Length} scouted locations to connection {ConnectionId}", scouts.Length, connectionId);

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
                    bool exists = await _hintService.HintExistsForLocationAsync(item.Location, item.Player);
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

                await _hintService.AddOrUpdateHintAsync(item.Player, hint);
                await _announcementService.AnnounceHintAsync(item.Player, findingPlayer, item.Item, item.Location, item.Flags);
                hintsCreated++;
            }

            if (hintsCreated > 0)
            {
                Logger.LogDebug("Created {Count} hints from location scouts (CreateAsHint={Mode})", hintsCreated, createAsHint);
            }
        }
    }
}
