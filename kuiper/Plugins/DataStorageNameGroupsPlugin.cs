using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class DataStorageNameGroupsPlugin : IPlugin
    {
        private static readonly Regex ItemGroupsRegex = new("^_read_item_name_groups_(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex LocationGroupsRegex = new("^_read_location_name_groups_(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ILogger<DataStorageNameGroupsPlugin> _logger;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly MultiData _multiData;

        public DataStorageNameGroupsPlugin(ILogger<DataStorageNameGroupsPlugin> logger, WebSocketConnectionManager connectionManager, MultiData multiData)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not Get getPacket)
                return;

            var responses = new Dictionary<string, JsonNode>();

            foreach (var key in getPacket.Keys)
            {
                if (TryHandleItemGroups(key, out var itemResult))
                {
                    responses[key] = itemResult;
                }
                else if (TryHandleLocationGroups(key, out var locResult))
                {
                    responses[key] = locResult;
                }
            }

            if (responses.Count > 0)
            {
                var retrieved = new Retrieved(responses);
                await _connectionManager.SendJsonToConnectionAsync(connectionId, new Packet[] { retrieved });
            }
        }

        private bool TryHandleItemGroups(string key, out JsonNode result)
        {
            result = null!;
            var match = ItemGroupsRegex.Match(key);
            if (!match.Success)
                return false;

            var game = match.Groups[1].Value;
            if (_multiData.DataPackage.TryGetValue(game, out var package) && package.ItemNameGroups != null)
            {
                result = JsonSerializer.SerializeToNode(package.ItemNameGroups)!;
                return true;
            }

            return false;
        }

        private bool TryHandleLocationGroups(string key, out JsonNode result)
        {
            result = null!;
            var match = LocationGroupsRegex.Match(key);
            if (!match.Success)
                return false;

            var game = match.Groups[1].Value;
            if (_multiData.DataPackage.TryGetValue(game, out var package) && package.LocationNameGroups != null)
            {
                result = JsonSerializer.SerializeToNode(package.LocationNameGroups)!;
                return true;
            }

            return false;
        }
    }
}
