using kbo.littlerocks;

namespace kuiper.Services.Abstract
{
    /// <summary>
    /// Service for broadcasting server announcements and messages to connected clients.
    /// </summary>
    public interface IServerAnnouncementService
    {
        /// <summary>
        /// Announces that a player has connected to the server.
        /// </summary>
        Task AnnouncePlayerConnectedAsync(long slotId, string playerName);

        /// <summary>
        /// Announces that a player has disconnected from the server.
        /// </summary>
        Task AnnouncePlayerDisconnectedAsync(long slotId, string playerName);

        /// <summary>
        /// Announces that a player has reached their goal.
        /// </summary>
        Task AnnounceGoalReachedAsync(long slotId, string playerName);

        /// <summary>
        /// Announces that an item was sent from one player to another.
        /// </summary>
        Task AnnounceItemSentAsync(long senderSlotId, long receiverSlotId, string itemName, long itemId, long locationId);

        /// <summary>
        /// Broadcasts a chat message from a player to all connected clients.
        /// </summary>
        Task BroadcastChatMessageAsync(long senderSlotId, string message);

        /// <summary>
        /// Broadcasts a server message to all connected clients.
        /// </summary>
        Task BroadcastServerMessageAsync(string message);

        /// <summary>
        /// Announces that a hint was found.
        /// </summary>
        Task AnnounceHintAsync(long receivingSlotId, long findingSlotId, long itemId, long locationId, NetworkItemFlags flags);
    }
}
