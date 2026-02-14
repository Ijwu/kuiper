using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Nodes;

using kbo.bigrocks;
using kuiper.Core.Pickle;
using Microsoft.Extensions.Logging;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

namespace kuiper.Core.DataStorage.Plugins
{
    public class DataStorageNameGroupsPlugin : BasePlugin
    {
        private static readonly Regex ItemGroupsRegex = new("^_read_item_name_groups_(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex LocationGroupsRegex = new("^_read_location_name_groups_(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly MultiData _multiData;

        public DataStorageNameGroupsPlugin(ILogger<DataStorageNameGroupsPlugin> logger, IConnectionManager connectionManager, MultiData multiData)
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
            Dictionary<string, JsonNode> responses = [];

            foreach (string key in getPacket.Keys)
            {
                if (TryHandleItemGroups(key, out JsonNode? itemResult))
                {
                    responses[key] = itemResult;
                }
                else if (TryHandleLocationGroups(key, out JsonNode? locResult))
                {
                    responses[key] = locResult;
                }
            }

            Retrieved retrieved = new(responses);
            await SendToConnectionAsync(connectionId, retrieved);
        }

        private bool TryHandleItemGroups(string key, out JsonNode result)
        {
            result = null!;

            Match match = ItemGroupsRegex.Match(key);
            if (!match.Success)
            {
                return false;
            }

            string game = match.Groups[1].Value;
            if (_multiData.DataPackage.TryGetValue(game, out MultiDataGamesPackage? package) && package.ItemNameGroups != null)
            {
                result = JsonSerializer.SerializeToNode(package.ItemNameGroups)!;
                return true;
            }

            return false;
        }

        private bool TryHandleLocationGroups(string key, out JsonNode result)
        {
            result = null!;

            Match match = LocationGroupsRegex.Match(key);
            if (!match.Success)
            {
                return false;
            }

            string game = match.Groups[1].Value;
            if (_multiData.DataPackage.TryGetValue(game, out MultiDataGamesPackage? package) && package.LocationNameGroups != null)
            {
                result = JsonSerializer.SerializeToNode(package.LocationNameGroups)!;
                return true;
            }

            return false;
        }
    }
}
