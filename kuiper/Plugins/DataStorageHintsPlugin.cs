using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class DataStorageHintsPlugin : IPlugin
    {
        private static readonly Regex HintKeyRegex = new("^_read_hints_0_(?<slot>\\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ILogger<DataStorageHintsPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly IHintService _hintService;

        public DataStorageHintsPlugin(ILogger<DataStorageHintsPlugin> logger, WebSocketConnectionManager connectionManager, IHintService hintService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not Get getPacket)
                return;

            Dictionary<string, JsonNode> results = new();

            foreach (var key in getPacket.Keys)
            {
                var match = HintKeyRegex.Match(key);
                if (!match.Success)
                    continue;

                if (!long.TryParse(match.Groups["slot"].Value, out var slotId))
                    continue;

                try
                {
                    var hints = await _hintService.GetHintsAsync(slotId);
                    var node = JsonSerializer.SerializeToNode(hints);
                    if (node != null)
                    {
                        results[key] = node;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve hints for slot {Slot}", slotId);
                }
            }

            if (results.Count > 0)
            {
                var response = new Retrieved(results);
                await _connectionManager.SendJsonToConnectionAsync(connectionId, new Packet[] { response });
            }
        }
    }
}
