using kbo.bigrocks;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.ConnectHandler.Plugins
{
    /// <summary>
    /// Stores tags from Connect and ConnectUpdate packets into the storage service keyed by connection id.
    /// </summary>
    public class ConnectionTagsPlugin : BasePlugin
    {
        private readonly IStorageService _storage;

        public ConnectionTagsPlugin(ILogger<ConnectionTagsPlugin> logger, IConnectionManager connectionManager, IStorageService storage)
            : base(logger, connectionManager)
        {
            _storage = storage;
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
            string[] value = tags ?? Array.Empty<string>();
            await _storage.SaveAsync(StorageKeys.ConnectionTags(connectionId), value);
            Logger.LogDebug("Connection ({ConnectionId}) switched tags to ({Tags}).", connectionId, value);
        }
    }
}
