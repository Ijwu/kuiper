using System.Text.Json;
using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;
using Microsoft.Extensions.DependencyInjection;
using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

namespace kuiper.Commands
{
    public class HintCommand : IConsoleCommand
    {
        public string Name => "hint";

        public string Description => "Create a hint for a slot by item name: hint <slotId> <item name>";

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                return "Usage: hint <slotId> <item name>";
            }

            var multiData = services.GetRequiredService<MultiData>();
            var hintService = services.GetRequiredService<IHintService>();
            var storage = services.GetRequiredService<IStorageService>();
            var connectionManager = services.GetRequiredService<WebSocketConnectionManager>();

            if (!long.TryParse(args[0], out var slotId))
            {
                return "Invalid slot id.";
            }

            if (!multiData.SlotInfo.TryGetValue((int)slotId, out var slotInfo))
            {
                return $"Unknown slot {slotId}.";
            }

            var game = slotInfo.Game;
            if (!multiData.DataPackage.TryGetValue(game, out var package))
            {
                return $"No datapackage for game {game}.";
            }

            var query = string.Join(' ', args.Skip(1));
            var matches = ResolveItemMatches(package.ItemNameToId, query);
            if (matches.Count == 0)
            {
                return $"Item '{query}' not found for game {game}.";
            }
            if (matches.Count > 1)
            {
                var multi = string.Join(Environment.NewLine, matches.Select(m => " - " + m.Key));
                return "Multiple matches found:" + Environment.NewLine + multi;
            }

            var itemName = matches.Single().Key;
            var itemId = matches.Single().Value;

            if (!multiData.Locations.TryGetValue(slotId, out var locations))
            {
                return $"No locations for slot {slotId}.";
            }

            var match = locations.FirstOrDefault(kvp => kvp.Value.Length >= 3 && kvp.Value[0] == itemId);
            if (match.Key == 0 && match.Value == null)
            {
                return $"No location contains item '{itemName}' (id {itemId}) for slot {slotId}.";
            }

            var locId = match.Key;
            var data = match.Value;
            var receivingPlayer = data[1];
            var itemFlags = (NetworkItemFlags)data[2];

            var locationName = package.LocationNameToId.FirstOrDefault(kvp => kvp.Value == locId).Key ?? locId.ToString();

            var existingHints = await hintService.GetHintsAsync(slotId);
            var existing = existingHints.FirstOrDefault(h => h.Location == locId && h.Item == itemId && h.ReceivingPlayer == receivingPlayer);
            if (existing is not null)
            {
                var statusExisting = await hintService.GetHintStatusAsync(slotId, existing);
                return $"Hint already exists for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, receiving player {receivingPlayer}), status {statusExisting}.";
            }

            var status = itemFlags.HasFlag(NetworkItemFlags.Trap) ? HintStatus.Avoid : HintStatus.Priority;
            var hint = new Hint(receivingPlayer, slotId, locId, itemId, found: false, entrance: string.Empty, itemFlags: itemFlags);
            await hintService.AddHintAsync(slotId, hint, status);

            await NotifySubscribersAsync(slotId, hintService, storage, connectionManager);

            return $"Hint created for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, receiving player {receivingPlayer}).";
        }

        private static async Task NotifySubscribersAsync(long slotId, IHintService hintService, IStorageService storage, WebSocketConnectionManager connectionManager)
        {
            const string notifyPrefix = "#setnotify:";
            var readKey = $"_read_hints_0_{slotId}";

            var keys = await storage.ListKeysAsync();
            foreach (var key in keys)
            {
                if (!key.StartsWith(notifyPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var connectionId = key.Substring(notifyPrefix.Length);
                var subscriptions = await storage.LoadAsync<string[]>(key) ?? Array.Empty<string>();
                if (!subscriptions.Any(k => string.Equals(k, readKey, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var hints = await hintService.GetHintsAsync(slotId);
                var node = JsonSerializer.SerializeToNode(hints);
                if (node == null)
                    continue;

                var reply = new SetReply(readKey, node, node, slotId);
                await connectionManager.SendJsonToConnectionAsync(connectionId, new Packet[] { reply });
            }
        }

        private static List<KeyValuePair<string, long>> ResolveItemMatches(Dictionary<string, long> itemNameToId, string query)
        {
            var matches = itemNameToId
                .Where(kvp => kvp.Key.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // prioritize exact match at front if it exists
            var exact = itemNameToId.Where(kvp => kvp.Key.Equals(query, StringComparison.OrdinalIgnoreCase)).ToList();
            if (exact.Count == 1)
            {
                return exact;
            }

            return matches;
        }
    }
}
