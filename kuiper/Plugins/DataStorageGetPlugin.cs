using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class DataStorageGetPlugin : BasePlugin
    {
        private readonly IStorageService _storage;

        public DataStorageGetPlugin(ILogger<DataStorageGetPlugin> logger, WebSocketConnectionManager connectionManager, IStorageService storage)
            : base(logger, connectionManager)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void RegisterHandlers()
        {
            Handle<Get>(HandleGetAsync);
        }

        private async Task HandleGetAsync(Get getPacket, string connectionId)
        {
            Dictionary<string, object> responseData = new();
            foreach (var key in getPacket.Keys)
            {
                if (key.StartsWith("_")) // handle internal keys with other plugins
                    continue;

                var data = await _storage.LoadAsync<object>(key);
                if (data != null)
                {
                    responseData.Add(key, data);
                }
                else
                {
                    Logger.LogDebug("No data found for key '{Key}' requested by connection {ConnectionId}", key, connectionId);
                }
            }

            if (responseData.Count > 0)
            {
                var responsePacket = new Retrieved(responseData.ToDictionary(kvp => kvp.Key, kvp => JsonSerializer.SerializeToNode(kvp.Value)));
                await SendToConnectionAsync(connectionId, responsePacket);
            }
        }
    }
}
