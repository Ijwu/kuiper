using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    /// <summary>
    /// Plugin that handles chat messages (SayPacket) from clients and broadcasts them to all connected players.
    /// </summary>
    public class ChatPlugin : IPlugin
    {
        private readonly ILogger<ChatPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IServerAnnouncementService _announcementService;

        public ChatPlugin(
            ILogger<ChatPlugin> logger,
            WebSocketConnectionManager connectionManager,
            IServerAnnouncementService announcementService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not Say sayPacket)
                return;

            if (sayPacket.Text.StartsWith("!"))
                return;

            try
            {
                var slotId = await _connectionManager.GetSlotForConnectionAsync(connectionId);

                if (!slotId.HasValue)
                {
                    _logger.LogDebug("Received SayPacket from connection {ConnectionId} with no mapped slot; ignoring.", connectionId);
                    return;
                }

                if (string.IsNullOrWhiteSpace(sayPacket.Text))
                {
                    _logger.LogDebug("Received empty SayPacket from connection {ConnectionId}; ignoring.", connectionId);
                    return;
                }

                await _announcementService.BroadcastChatMessageAsync(slotId.Value, sayPacket.Text);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling SayPacket from connection {ConnectionId}", connectionId);
            }
        }
    }
}
