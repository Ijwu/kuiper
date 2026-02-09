using kbo.littlerocks;

namespace kuiper.Core.Services.Abstract
{
    /// <summary>
    /// Service for broadcasting server announcements and messages to connected clients.
    /// </summary>
    public interface IServerAnnouncementService
    {
        Task AnnounceHintAsync(long receivingPlayer, long findingPlayer, long itemId, long locationId, NetworkItemFlags itemFlags);
        Task AnnounceItemSentAsync(long slotId, long receivingPlayer, string itemName, long itemId, long locationId);

        /// <summary>
        /// Announces that a player has connected to the server.
        /// </summary>
        Task AnnouncePlayerConnectedAsync(long slotId, string playerName);

        /// <summary>
        /// Announces that a player has disconnected from the server.
        /// </summary>
        Task AnnouncePlayerDisconnectedAsync(long slotId, string playerName);
    }
}
