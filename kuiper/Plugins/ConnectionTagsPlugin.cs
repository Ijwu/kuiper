using kbo.bigrocks;
using kuiper.Services.Abstract;
using kuiper.Services;

namespace kuiper.Plugins
{
    /// <summary>
    /// Stores tags from Connect and ConnectUpdate packets into the storage service keyed by connection id.
    /// </summary>
    public class ConnectionTagsPlugin : BasePlugin
    {
        private const string KeyPrefix = "#connection_tags:";

        private readonly IStorageService _storage;

        public ConnectionTagsPlugin(ILogger<ConnectionTagsPlugin> logger, WebSocketConnectionManager connectionManager, IStorageService storage)
            : base(logger, connectionManager)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void RegisterHandlers()
        {
            Handle<Connect>(HandleConnectAsync);
            Handle<ConnectUpdate>(HandleConnectUpdateAsync);
        }

        private Task HandleConnectAsync(Connect packet, string connectionId)
        {
            return StoreTagsAsync(connectionId, packet.Tags);
        }

        private Task HandleConnectUpdateAsync(ConnectUpdate packet, string connectionId)
        {
            return StoreTagsAsync(connectionId, packet.Tags);
        }

        private async Task StoreTagsAsync(string connectionId, string[]? tags)
        {
            try
            {
                var key = KeyPrefix + connectionId;
                await _storage.SaveAsync(key, tags ?? Array.Empty<string>());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to store tags for connection {ConnectionId}", connectionId);
            }
        }
    }
}
