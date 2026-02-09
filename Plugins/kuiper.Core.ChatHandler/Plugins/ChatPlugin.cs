using kbo.bigrocks;

using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.ChatHandler.Plugins
{
    /// <summary>
    /// Plugin that handles chat messages (SayPacket) from clients and broadcasts them to all connected players.
    /// </summary>
    public class ChatPlugin : BasePlugin
    {
        private readonly IServerAnnouncementService _announcementService;

        public ChatPlugin(
            ILogger<ChatPlugin> logger,
            IConnectionManager connectionManager,
            IServerAnnouncementService announcementService)
            : base(logger, connectionManager)
        {
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
        }

        protected override void RegisterHandlers()
        {
            Handle<Say>(HandleSayAsync);
        }

        private async Task HandleSayAsync(Say sayPacket, string connectionId)
        {
            //TODO: use configured command prefix
            if (sayPacket.Text.StartsWith("!"))
                return;

            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
                return;

            if (string.IsNullOrWhiteSpace(sayPacket.Text))
            {
                Logger.LogDebug("Received empty SayPacket from connection {ConnectionId}; ignoring.", connectionId);
                return;
            }

            await _announcementService.BroadcastChatMessageAsync(slotId, sayPacket.Text);
        }
    }
}
