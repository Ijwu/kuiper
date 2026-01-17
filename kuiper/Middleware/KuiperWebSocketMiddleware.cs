using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Middleware
{
    public class KuiperWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<KuiperWebSocketMiddleware> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IWebSocketHandler _webSocketHandler;

        public KuiperWebSocketMiddleware(
            RequestDelegate next,
            ILogger<KuiperWebSocketMiddleware> logger,
            WebSocketConnectionManager connectionManager,
            IWebSocketHandler webSocketHandler)
        {
            _next = next;
            _logger = logger;
            _connectionManager = connectionManager;
            _webSocketHandler = webSocketHandler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var connectionId = Guid.NewGuid().ToString();

                _logger.LogInformation("Incoming Connection. Connection ID: {ConnectionId}", connectionId);

                var newPlayer = new PlayerData
                {
                    Socket = webSocket
                };

                await _connectionManager.AddConnectionAsync(connectionId, newPlayer);
                await _webSocketHandler.HandleConnectionAsync(connectionId, newPlayer);
            }
            else
            {
                // Previous behavior was to return 400 if not a WebSocket request mapped to "/"
                // Since this middleware is intended for the WebSocket endpoint, we enforce strict protocol check.
                // If we allowed _next(context), it would fall through to 404 or other handlers.
                // Keeping existing behavior of 400 Bad Request for explicit rejection.
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
