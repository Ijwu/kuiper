using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class DataStorageRaceModePlugin : IPlugin
    {
        private readonly ILogger<DataStorageRaceModePlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;

        public DataStorageRaceModePlugin(ILogger<DataStorageRaceModePlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not Get getPacket)
                return;

            if (!getPacket.Keys.Any(k => string.Equals(k, "_read_race_mode", StringComparison.OrdinalIgnoreCase)))
                return;

            try
            {
                var node = JsonSerializer.SerializeToNode(_multiData.RaceMode) ?? JsonValue.Create(_multiData.RaceMode);
                var response = new Retrieved(new Dictionary<string, JsonNode> { { "_read_race_mode", node! } });
                await _connectionManager.SendJsonToConnectionAsync(connectionId, new Packet[] { response });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle _read_race_mode request");
            }
        }
    }
}
