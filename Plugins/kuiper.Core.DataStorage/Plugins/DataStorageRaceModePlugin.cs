using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.DataStorage.Plugins
{
    public class DataStorageRaceModePlugin : BasePlugin
    {
        private readonly MultiData _multiData;

        public DataStorageRaceModePlugin(ILogger<DataStorageRaceModePlugin> logger, IConnectionManager connectionManager, MultiData multiData)
            : base(logger, connectionManager)
        {
            _multiData = multiData;
        }

        protected override void RegisterHandlers()
        {
            Handle<Get>(HandleGetAsync);
        }

        private async Task HandleGetAsync(Get getPacket, string connectionId)
        {
            if (!getPacket.Keys.Any(k => string.Equals(k, "_read_race_mode", StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            JsonValue node = JsonValue.Create(_multiData.RaceMode);
            Retrieved response = new(new Dictionary<string, JsonNode> { { "_read_race_mode", node! } });
            await SendToConnectionAsync(connectionId, response);
        }
    }
}
