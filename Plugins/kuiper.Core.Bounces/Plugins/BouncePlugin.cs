using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Constants;
using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Bounces.Plugins
{
    /// <summary>
    /// Handles Bounce packets by forwarding them as Bounced to matching clients (games/slots/tags).
    /// </summary>
    public class BouncePlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly INotifyingStorageService _storage;

        public BouncePlugin(ILogger<BouncePlugin> logger, IConnectionManager connectionManager, MultiData multiData, INotifyingStorageService storage)
            : base(logger, connectionManager)
        {
            _multiData = multiData;
            _storage = storage;
        }

        protected override void RegisterHandlers()
        {
            Handle<Bounce>(HandleBounceAsync);
        }

        private async Task HandleBounceAsync(Bounce bounce, string connectionId)
        {
            IEnumerable<string> recipients = await ConnectionManager.GetAllConnectionIdsAsync();
            foreach (var connId in recipients)
            {
                long? slotId = await ConnectionManager.GetSlotForConnectionAsync(connId);

                if (slotId == null)
                {
                    continue;
                }

                string[] receiverTags = await _storage.LoadAsync<string[]>(StorageKeys.ConnectionTags(connId)) ?? [];
                string? receiverGame = slotId.HasValue && _multiData.SlotInfo.TryGetValue((int)slotId.Value, out var slot) ? slot.Game : null;

                if (!MatchesFilters(bounce, slotId.Value, receiverGame, receiverTags))
                {
                    continue;
                }

                Bounced bounced = new(bounce.Games ?? [], bounce.Slots ?? [], bounce.Tags ?? [], bounce.Data ?? new());
                await ConnectionManager.SendJsonToConnectionAsync(connId, new Packet[] { bounced });
            }
        }

        private static bool MatchesFilters(Bounce bounce, long receiverSlot, string? receiverGame, string[] receiverTags)
        {
            bool anyMatch = false;
            if (bounce.Games.Length > 0)
            {
                if (!string.IsNullOrEmpty(receiverGame))
                {
                    if (bounce.Games.Contains(receiverGame))
                    {
                        anyMatch = true;
                    }
                }
            }

            if (bounce.Slots.Length > 0)
            {
                if (bounce.Slots.Contains(receiverSlot))
                {
                    anyMatch = true;
                }
            }

            if (bounce.Tags.Length > 0)
            {
                if (bounce.Tags.Any(tag => receiverTags.Contains(tag)))
                {
                    anyMatch = true;
                }
            }

            return anyMatch;
        }
    }
}
