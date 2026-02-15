using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Hints.Plugins
{
    public class CreateHintsPlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly IHintService _hintService;
        private readonly IServerAnnouncementService _announcementService;

        public CreateHintsPlugin(ILogger<CreateHintsPlugin> logger, IConnectionManager connectionManager, MultiData multiData, IHintService hintService, IServerAnnouncementService announcementService)
            : base(logger, connectionManager)
        {
            _multiData = multiData;
            _hintService = hintService;
            _announcementService = announcementService;
        }

        protected override void RegisterHandlers()
        {
            Handle<CreateHints>(HandleCreateHintsAsync);
        }

        private async Task HandleCreateHintsAsync(CreateHints createHints, string connectionId)
        {
            if (createHints.Locations is null || createHints.Locations.Length == 0)
            {
                return;
            }

            (bool success, long slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
            {
                return;
            }

            long targetSlotId = createHints.Player != 0 ? createHints.Player : slotId;
            if (!_multiData.Locations.TryGetValue(targetSlotId, out var slotLocations))
            {
                Logger.LogDebug("No location data found for slot {Slot} while handling CreateHints.", targetSlotId);
                return;
            }
            HintStatus hintStatus = createHints.Status;

            if (hintStatus == HintStatus.Found)
            {
                return;
            }

            foreach (long loc in createHints.Locations)
            {
                if (!slotLocations.TryGetValue(loc, out var data) || data.Length < 3)
                {
                    Logger.LogDebug("Location {Location} not found for slot {Slot} while handling CreateHints.", loc, targetSlotId);
                    continue;
                }

                var itemId = data[0];
                var receivingPlayer = data[1];
                var itemFlags = (NetworkItemFlags)data[2];
                var status = itemFlags.HasFlag(NetworkItemFlags.Trap) ? HintStatus.Avoid : hintStatus;

                var hint = new Hint(receivingPlayer, targetSlotId, loc, itemId, found: false, entrance: string.Empty, itemFlags: itemFlags, status: status);
                await _hintService.AddOrUpdateHintAsync(targetSlotId, hint);
                await _announcementService.AnnounceHintAsync(receivingPlayer, targetSlotId, itemId, loc, itemFlags);
            }
        }
    }
}
