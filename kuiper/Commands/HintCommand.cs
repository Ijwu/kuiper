using System.Text.Json;
using System.Text.Json.Nodes;
using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;
using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

namespace kuiper.Commands
{
    public class HintCommand : IConsoleCommand
    {
        public string Name => "hint";

        public string Description => "Create a hint for a slot by item name: hint <slotId> <item name>";

        private readonly MultiData _multiData;
        private readonly IHintService _hintService;

        public HintCommand(MultiData multiData, IHintService hintService)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
        }

        public async Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: hint <slotId> <item name>");
                return;
            }

            if (!long.TryParse(args[0], out var slotId))
            {
                Console.WriteLine("Invalid slot id.");
                return;
            }

            if (!_multiData.SlotInfo.TryGetValue((int)slotId, out var slotInfo))
            {
                Console.WriteLine($"Unknown slot {slotId}.");
                return;
            }

            var game = slotInfo.Game;
            if (!_multiData.DataPackage.TryGetValue(game, out var package))
            {
                Console.WriteLine($"No datapackage for game {game}.");
                return;
            }

            var query = string.Join(' ', args.Skip(1));
            var matches = ResolveItemMatches(package.ItemNameToId, query);
            if (matches.Count == 0)
            {
                Console.WriteLine($"Item '{query}' not found for game {game}.");
                return;
            }
            if (matches.Count > 1)
            {
                Console.WriteLine("Multiple matches found:");
                foreach (var m in matches)
                {
                    Console.WriteLine(" - " + m.Key);
                }
                return;
            }

            var itemName = matches.Single().Key;
            var itemId = matches.Single().Value;

            if (!_multiData.Locations.TryGetValue(slotId, out var locations))
            {
                Console.WriteLine($"No locations for slot {slotId}.");
                return;
            }

            var match = locations.FirstOrDefault(kvp => kvp.Value.Length >= 3 && kvp.Value[0] == itemId);
            if (match.Key == 0 && match.Value == null)
            {
                Console.WriteLine($"No location contains item '{itemName}' (id {itemId}) for slot {slotId}.");
                return;
            }

            var locId = match.Key;
            var data = match.Value;
            var receivingPlayer = data[1];
            var itemFlags = (NetworkItemFlags)data[2];

            var locationName = package.LocationNameToId.FirstOrDefault(kvp => kvp.Value == locId).Key ?? locId.ToString();

            var existingHints = await _hintService.GetHintsAsync(slotId);
            var existing = existingHints.FirstOrDefault(h => h.Location == locId && h.Item == itemId && h.ReceivingPlayer == receivingPlayer);
            if (existing is not null)
            {
                var status = await _hintService.GetHintStatusAsync(slotId, existing);
                Console.WriteLine($"Hint already exists for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, receiving player {receivingPlayer}), status {status}.");
                return;
            }

            var hint = new Hint(receivingPlayer, slotId, locId, itemId, found: false, entrance: string.Empty, itemFlags: itemFlags);
            await _hintService.AddHintAsync(slotId, hint, HintStatus.Unspecified);

            Console.WriteLine($"Hint created for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, receiving player {receivingPlayer}).");

            await NotifySubscribersAsync(slotId, hint, services, itemName, locationName);
        }

        private static async Task NotifySubscribersAsync(long slotId, Hint hint, IServiceProvider services, string itemName, string locationName)
        {
            var storage = services.GetRequiredService<IStorageService>();
            var connectionManager = services.GetRequiredService<WebSocketConnectionManager>();
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

                var hints = await services.GetRequiredService<IHintService>().GetHintsAsync(slotId);
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
