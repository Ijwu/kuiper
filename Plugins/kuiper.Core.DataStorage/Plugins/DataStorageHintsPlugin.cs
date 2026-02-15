using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.DataStorage.Plugins
{
    public class DataStorageHintsPlugin : BasePlugin
    {
        private static readonly Regex HintKeyRegex = new("^_read_hints_\\d+_(?<slot>\\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IHintService _hintService;

        public DataStorageHintsPlugin(ILogger<DataStorageHintsPlugin> logger, IConnectionManager connectionManager, IHintService hintService)
            : base(logger, connectionManager)
        {
            _hintService = hintService;
        }

        protected override void RegisterHandlers()
        {
            Handle<Get>(HandleGetAsync);
        }

        private async Task HandleGetAsync(Get getPacket, string connectionId)
        {
            Dictionary<string, JsonNode> results = [];

            foreach (string key in getPacket.Keys)
            {
                Match match = HintKeyRegex.Match(key);
                if (!match.Success)
                {
                    continue;
                }

                if (!long.TryParse(match.Groups["slot"].Value, out long slotId))
                {
                    continue;
                }

                Hint[] hints = await _hintService.GetHintsAsync(slotId);
                JsonNode? node = JsonSerializer.SerializeToNode(hints);
                if (node != null)
                {
                    results[key] = node;
                }
            }

            Retrieved response = new(results);
            await SendToConnectionAsync(connectionId, response);
        }
    }
}
