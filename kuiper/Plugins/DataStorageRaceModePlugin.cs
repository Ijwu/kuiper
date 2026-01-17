using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class DataStorageRaceModePlugin : BasePlugin
    {
        private readonly MultiData _multiData;

        public DataStorageRaceModePlugin(ILogger<DataStorageRaceModePlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        protected override void RegisterHandlers()
        {
            Handle<Get>(HandleGetAsync);
        }

        private async Task HandleGetAsync(Get getPacket, string connectionId)
        {
            if (!getPacket.Keys.Any(k => string.Equals(k, "_read_race_mode", StringComparison.OrdinalIgnoreCase)))
                return;

            var node = JsonSerializer.SerializeToNode(_multiData.RaceMode) ?? JsonValue.Create(_multiData.RaceMode);
            var response = new Retrieved(new Dictionary<string, JsonNode> { { "_read_race_mode", node! } });
            await SendToConnectionAsync(connectionId, response);
        }
    }
}
