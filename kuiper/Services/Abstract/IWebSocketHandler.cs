namespace kuiper.Services.Abstract
{
    /// <summary>
    /// Handles WebSocket connection lifecycle and message processing.
    /// </summary>
    public interface IWebSocketHandler
    {
        /// <summary>
        /// Handles an established WebSocket connection, processing messages until the connection closes.
        /// </summary>
        /// <param name="connectionId">Unique identifier for the connection.</param>
        /// <param name="player">Player data associated with this connection.</param>
        Task HandleConnectionAsync(string connectionId, PlayerData player);
    }
}
