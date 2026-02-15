using System.Text.Json;

using kbo.bigrocks;

using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.DataStorage.Plugins
{
    public class DataStorageGetPlugin : BasePlugin
    {
        private readonly INotifyingStorageService _storage;

        public DataStorageGetPlugin(ILogger<DataStorageGetPlugin> logger, IConnectionManager connectionManager, INotifyingStorageService storage)
            : base(logger, connectionManager)
        {
            _storage = storage;
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
                if (key.StartsWith("_read_") || key.StartsWith("#")) // handle internal keys with other plugins
                {
                    continue;
                }

                var data = await _storage.LoadAsync<object>(key);

                // Client should receive nulls
                responseData.Add(key, data!);
            }

            Retrieved responsePacket = new(responseData.ToDictionary(kvp => kvp.Key, kvp => JsonSerializer.SerializeToNode(kvp.Value)!));
            await SendToConnectionAsync(connectionId, responsePacket);
        }
    }
}
