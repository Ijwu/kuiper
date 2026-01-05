using kbo.bigrocks;
using kbo.littlerocks;
using kuiper.Services.Abstract;
using kuiper.Services;

namespace kuiper.Plugins
{
    /// <summary>
    /// Stores tags from Connect and ConnectUpdate packets into the storage service keyed by connection id.
    /// </summary>
    public class ConnectionTagsPlugin : IPlugin
    {
        private const string KeyPrefix = "#connection_tags:";

        private readonly ILogger<ConnectionTagsPlugin> _logger;
        private readonly IStorageService _storage;

        public ConnectionTagsPlugin(ILogger<ConnectionTagsPlugin> logger, IStorageService storage)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            switch (packet)
            {
                case Connect connect:
                    await StoreTagsAsync(connectionId, connect.Tags);
                    break;
                case ConnectUpdate update:
                    await StoreTagsAsync(connectionId, update.Tags);
                    break;
            }
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
                _logger.LogError(ex, "Failed to store tags for connection {ConnectionId}", connectionId);
            }
        }
    }
}
