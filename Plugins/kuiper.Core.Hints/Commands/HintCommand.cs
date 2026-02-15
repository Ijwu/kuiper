using System.Text.Json;

using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Commands.Abstract;
using kuiper.Core.Constants;
using kuiper.Core.Extensions;
using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

namespace kuiper.Core.Hints.Commands
{
    public class HintCommand : ICommand
    {
        private readonly MultiData _multiData;
        private readonly IHintService _hintService;
        private readonly INotifyingStorageService _storage;
        private readonly IConnectionManager _connectionManager;
        private readonly IServerAnnouncementService _serverAnnouncementService;
        private readonly IHintPointsService _hintPointsService;

        public HintCommand(MultiData multiData,
                           IHintService hintService,
                           INotifyingStorageService storage,
                           IConnectionManager connectionManager,
                           IServerAnnouncementService serverAnnouncementService,
                           IHintPointsService hintPointsService)
        {
            _multiData = multiData;
            _hintService = hintService;
            _storage = storage;
            _connectionManager = connectionManager;
            _serverAnnouncementService = serverAnnouncementService;
            _hintPointsService = hintPointsService;
        }
        public string Name => "hint";

        public string Description => "Create a hint for a slot by item name";

        public async Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken cancellationToken)
        {
            if (args.Length < 2 && executingSlot == -1)
            {
                return "Usage: hint <slotId_or_name> <item name>";
            }
            else if (args.Length < 1 && executingSlot != -1)
            {
                return "Usage: hint <item name>";
            }

            long? slotId = null;
            int hintCost = 0;
            if (executingSlot == -1)
            {
                string identifier = args[0];
                if (!_multiData.TryResolveSlotId(identifier, out slotId))
                {
                    return $"Unknown slot '{identifier}'.";
                }
            }
            else
            {
                slotId = executingSlot;

                int lcp = (int)_multiData.ServerOptions["location_check_points"];
                int totalChecksForSlot = _multiData.Locations[executingSlot].Count;
                int hintCostPercentage = (int)_multiData.ServerOptions["hint_cost"];
                long hintPointsForSlot = await _hintPointsService.GetHintPointsAsync(executingSlot);

                hintCost = (hintCostPercentage == 0) ? 0 : (totalChecksForSlot / (100 / (hintCostPercentage)));

                if (hintPointsForSlot < hintCost)
                {
                    return $"Not enough hint points. You need {hintCost} but you have only {hintPointsForSlot}. You get {lcp} hint points per completed check.";
                }
            }

            MultiDataNetworkSlot slotInfo = _multiData.SlotInfo[(int)slotId!];

            string game = slotInfo.Game;
            if (!_multiData.DataPackage.TryGetValue(game, out MultiDataGamesPackage? package))
            {
                return $"No datapackage for game {game}.";
            }

            string query = null!;
            if (executingSlot == -1)
            {
                query = string.Join(' ', args.Skip(1));
            }
            else
            {
                query = string.Join(' ', args);
            }
            
            List<KeyValuePair<string, long>> matches = ResolveItemMatches(package.ItemNameToId, query);
            if (matches.Count == 0)
            {
                return $"Item '{query}' not found for game {game}.";
            }

            if (matches.Count > 1)
            {
                var multi = string.Join(Environment.NewLine, matches.Select(m => " - " + m.Key));
                return "Multiple matches found:" + Environment.NewLine + multi;
            }

            string itemName = matches.Single().Key;
            long itemId = matches.Single().Value;

            KeyValuePair<long, long[]> match = default;
            bool foundMatch = false;
            long sendingPlayer = 0;
            foreach (KeyValuePair<long, Dictionary<long, long[]>> slotLocations in _multiData.Locations)
            {
                Dictionary<long, long[]> locations = slotLocations.Value;
                foreach (KeyValuePair<long, long[]> location in locations)
                {
                    if (location.Value[0] == itemId && location.Value[1] == slotId)
                    {
                        match = location;
                        foundMatch = true;
                        break;
                    }
                }

                if (foundMatch)
                {
                    sendingPlayer = slotLocations.Key;
                    break;
                }
            }

            if (!foundMatch)
            {
                return "Could not find hinted item in any world.";
            }

            long locId = match.Key;
            long[] data = match.Value;
            long receivingPlayer = data[1];
            NetworkItemFlags itemFlags = (NetworkItemFlags)data[2];

            string locationName = package.LocationNameToId.FirstOrDefault(kvp => kvp.Value == locId).Key ?? locId.ToString();

            Hint[] existingHints = await _hintService.GetHintsAsync(slotId.Value);
            Hint? existing = existingHints.FirstOrDefault(h => h.Location == locId && h.Item == itemId && h.FindingPlayer == sendingPlayer);
            if (existing is not null)
            {
                return $"Hint already exists for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, finding p[layer {sendingPlayer}), status {existing.Status}.";
            }

            HintStatus status = itemFlags.HasFlag(NetworkItemFlags.Trap) ? HintStatus.Avoid : HintStatus.Priority;
            Hint hint = new(receivingPlayer, sendingPlayer, locId, itemId, found: false, entrance: string.Empty, itemFlags: itemFlags, status: status);
            await _hintService.AddOrUpdateHintAsync(slotId.Value, hint);

            if (executingSlot != -1)
            {
                await _hintPointsService.AddHintPointsAsync(slotId.Value, -hintCost);
            }

            await NotifySubscribersAsync(slotId.Value, _hintService, _storage, _connectionManager);
            await _serverAnnouncementService.AnnounceHintAsync(receivingPlayer, sendingPlayer, itemId, locId, itemFlags);

            return $"Hint created for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, finding player {sendingPlayer}).";
        }

        private static async Task NotifySubscribersAsync(long slotId, IHintService hintService, INotifyingStorageService storage, IConnectionManager connectionManager)
        {
            var readKey = $"_read_hints_0_{slotId}";

            var keys = await storage.ListKeysAsync();
            foreach (var key in keys)
            {
                if (!key.StartsWith(StorageKeys.SetNotifyPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var connectionId = key.Substring(StorageKeys.SetNotifyPrefix.Length);
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
