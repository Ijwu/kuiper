using kuiper.Core.Services.Abstract;
using kuiper.Internal;

namespace kuiper.Middleware
{
    internal class KuiperWebSocketMiddleware
    {
        private readonly ILogger<KuiperWebSocketMiddleware> _logger;
        private readonly IConnectionManager _connectionManager;
        private readonly IWebSocketHandler _webSocketHandler;

        public KuiperWebSocketMiddleware(
            RequestDelegate next,
            ILogger<KuiperWebSocketMiddleware> logger,
            IConnectionManager connectionManager,
            IWebSocketHandler webSocketHandler)
        {
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

                _logger.LogDebug("Incoming Connection. Connection ID: {ConnectionId}", connectionId);

                await _connectionManager.AddConnectionAsync(connectionId, webSocket);
                await _webSocketHandler.HandleConnectionAsync(connectionId, webSocket);
            }
            else
            {
                // Don't fallthrough to any other middleware. Just reject all non-WebSocket connections with a 400.
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
