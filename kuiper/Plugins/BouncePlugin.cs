using System.Collections.Concurrent;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    /// <summary>
    /// Handles Bounce packets by forwarding them as Bounced to matching clients (games/slots/tags).
    /// Tracks client tags from Connect/ConnectUpdate.
    /// </summary>
    public class BouncePlugin : IPlugin
    {
        private readonly ILogger<BouncePlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;
        private readonly ConcurrentDictionary<string, string[]> _connectionTags = new();

        public BouncePlugin(ILogger<BouncePlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            switch (packet)
            {
                case Connect connect:
                    _connectionTags[connectionId] = connect.Tags ?? Array.Empty<string>();
                    break;
                case ConnectUpdate update:
                    _connectionTags[connectionId] = update.Tags ?? Array.Empty<string>();
                    break;
                case Bounce bounce:
                    await ForwardBounceAsync(bounce, connectionId);
                    break;
            }
        }

        private async Task ForwardBounceAsync(Bounce bounce, string senderConnectionId)
        {
            try
            {
                var recipients = _connectionManager.GetAllConnectionIds();
                foreach (var connectionId in recipients)
                {
                    var slotId = await _connectionManager.GetSlotForConnectionAsync(connectionId);
                    var receiverTags = _connectionTags.TryGetValue(connectionId, out var tags) ? tags : Array.Empty<string>();
                    var receiverGame = slotId.HasValue && _multiData.SlotInfo.TryGetValue((int)slotId.Value, out var slot) ? slot.Game : null;

                    if (!MatchesFilters(bounce, slotId, receiverGame, receiverTags))
                        continue;

                    var bounced = new Bounced(bounce.Games ?? [], bounce.Slots ?? [], bounce.Tags ?? [], bounce.Data ?? new());
                    await _connectionManager.SendJsonToConnectionAsync(connectionId, new Packet[] { bounced });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to forward Bounce packet");
            }
        }

        private static bool MatchesFilters(Bounce bounce, long? receiverSlot, string? receiverGame, string[] receiverTags)
        {
            if (bounce.Games is { Length: > 0 })
            {
                if (string.IsNullOrWhiteSpace(receiverGame) || !bounce.Games.Contains(receiverGame))
                    return false;
            }

            if (bounce.Slots is { Length: > 0 })
            {
                if (!receiverSlot.HasValue || !bounce.Slots.Contains(receiverSlot.Value))
                    return false;
            }

            if (bounce.Tags is { Length: > 0 })
            {
                if (receiverTags is null || !receiverTags.Any(t => bounce.Tags.Contains(t)))
                    return false;
            }

            return true;
        }
    }
}
