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
    public class DataStorageHintsPlugin : BasePlugin
    {
        private static readonly Regex HintKeyRegex = new("^_read_hints_0_(?<slot>\\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IHintService _hintService;

        public DataStorageHintsPlugin(ILogger<DataStorageHintsPlugin> logger, WebSocketConnectionManager connectionManager, IHintService hintService)
            : base(logger, connectionManager)
        {
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
        }

        protected override void RegisterHandlers()
        {
            Handle<Get>(HandleGetAsync);
        }

        private async Task HandleGetAsync(Get getPacket, string connectionId)
        {
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
                    Logger.LogError(ex, "Failed to retrieve hints for slot {Slot}", slotId);
                }
            }

            if (results.Count > 0)
            {
                var response = new Retrieved(results);
                await SendToConnectionAsync(connectionId, response);
            }
        }
    }
}
