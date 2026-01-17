using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class CreateHintsPlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly IHintService _hintService;

        public CreateHintsPlugin(ILogger<CreateHintsPlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData, IHintService hintService)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
        }

        protected override void RegisterHandlers()
        {
            Handle<CreateHints>(HandleCreateHintsAsync);
        }

        private async Task HandleCreateHintsAsync(CreateHints createHints, string connectionId)
        {
            if (createHints.Locations is null || createHints.Locations.Length == 0)
                return;

            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
                return;

            var targetSlotId = createHints.Player != 0 ? createHints.Player : slotId;
            if (!_multiData.Locations.TryGetValue(targetSlotId, out var slotLocations))
            {
                Logger.LogDebug("No location data found for slot {Slot} while handling CreateHints.", targetSlotId);
                return;
            }
            var hintStatus = createHints.Status;

            foreach (var loc in createHints.Locations)
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

                var hint = new Hint(receivingPlayer, targetSlotId, loc, itemId, found: false, entrance: string.Empty, itemFlags: itemFlags);
                await _hintService.AddHintAsync(targetSlotId, hint, status);
            }
        }
    }
}
