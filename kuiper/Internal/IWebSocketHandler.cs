using System.Net.WebSockets;

namespace kuiper.Internal
{
    internal interface IWebSocketHandler
    {
        Task HandleConnectionAsync(string connectionId, WebSocket connection);
    }
}
