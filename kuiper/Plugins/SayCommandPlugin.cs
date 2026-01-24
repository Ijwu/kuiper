using System.Text.Json;

using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Commands;
using kuiper.Constants;
using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

using static kbo.littlerocks.JsonMessagePart;

namespace kuiper.Plugins
{
    /// <summary>
    /// Handles Say packets that start with '!' by executing the remainder as a console command
    /// and returning the output to the sender as a PrintJson packet.
    /// </summary>
    public class SayCommandPlugin : BasePlugin
    {
        private readonly CommandRegistry _registry;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IStorageService _storage;
        private readonly IKuiperConfig _config;
        private readonly IHintService _hintService;
        private readonly IHintPointsService _hintPointsService;
        private readonly MultiData _multiData;

        public SayCommandPlugin(ILogger<SayCommandPlugin> logger,
                                CommandRegistry registry,
                                IServiceScopeFactory scopeFactory,
                                WebSocketConnectionManager connectionManager,
                                IStorageService storage,
                                IKuiperConfig config,
                                IHintService hintService,
                                IHintPointsService hintPointsService,
                                MultiData multiData)
            : base(logger, connectionManager)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
            _hintPointsService = hintPointsService ?? throw new ArgumentNullException(nameof(hintPointsService));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        protected override void RegisterHandlers()
        {
            Handle<Say>(HandleSayAsync);
        }

        private async Task HandleSayAsync(Say say, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(say.Text))
                return;

            var prefix = "!";
            try
            {
                var serverConfig = _config.GetServerConfig<ServerConfig>("Server");
                if (!string.IsNullOrEmpty(serverConfig?.IngameCommandPrefix))
                {
                    prefix = serverConfig.IngameCommandPrefix;
                }
            }
            catch
            {
                // Fallback to default
            }

            var text = say.Text.Trim();
            if (!text.StartsWith(prefix))
                return;

            var slotId = await ConnectionManager.GetSlotForConnectionAsync(connectionId);
            if (!slotId.HasValue)
            {
                await SendOutputAsync(connectionId, "Command rejected: connection not mapped to a slot.");
                return;
            }

            var commandLine = text[prefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(commandLine))
                return;

            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts.FirstOrDefault();
            var args = parts.Skip(1).ToArray();

            if (string.IsNullOrEmpty(commandName))
                return;

            var authorized = await _storage.LoadAsync<long[]>(StorageKeys.AuthorizedCommandSlots) ?? Array.Empty<long>();

            if (!authorized.Contains(slotId.Value))
            {
                // handle unauthorized password and hint commands
                if (commandName.Equals("password", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleUnauthorizedPasswordAsync(slotId.Value, args, connectionId);
                    return;
                }

                if (commandName.Equals("hint", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleUnauthorizedHintAsync(slotId.Value, args, connectionId);
                    return;
                }

                await SendOutputAsync(connectionId, "Command rejected: slot is not authorized.");
                return;
            }

            if (!_registry.TryGet(commandName, out var command))
            {
                await SendOutputAsync(connectionId, $"Unknown command '{commandName}'.");
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var services = scope.ServiceProvider;
                var result = await command.ExecuteAsync(args, services, CancellationToken.None);
                var output = string.IsNullOrWhiteSpace(result) ? "(no output)" : result;
                await SendOutputAsync(connectionId, output);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute command {Command}", commandName);
                await SendOutputAsync(connectionId, $"Command '{commandName}' failed: {ex.Message}");
            }
        }

        private async Task HandleUnauthorizedPasswordAsync(long slotId, string[] args, string connectionId)
        {
            var password = string.Join(" ", args);
            await _storage.SaveAsync(StorageKeys.Password(slotId), password);
            await SendOutputAsync(connectionId, "Slot password has been set.");
        }

        private async Task HandleUnauthorizedHintAsync(long slotId, string[] args, string connectionId)
        {
            if (args.Length == 0)
            {
                await SendOutputAsync(connectionId, "Command rejected: no item name provided.");
                return;
            }

            var totalChecksForSlot = _multiData.Locations[slotId].Count;
            var hintCostPercentage = (int)_multiData.ServerOptions["hint_cost"];
            var hintPointsForSlot = await _hintPointsService.GetHintPointsAsync(slotId);
            hintPointsForSlot = 50;

            if (hintCostPercentage != 0)
            {
                var hintCost = (totalChecksForSlot / (100 / hintCostPercentage));

                if (hintPointsForSlot >= hintCost)
                {
                    //await _hintPointsService.AddHintPointsAsync(slotId, -hintCost);
                }
                else
                {
                    await SendOutputAsync(connectionId, $"Command rejected: You need {hintCost} hint points. You have {await _hintPointsService.GetHintPointsAsync(slotId)}.");
                    return;
                }
            }

            var query = string.Join(' ', args);
            var package = _multiData.DataPackage[_multiData.SlotInfo[(int)slotId].Game];
            var matches = ResolveItemMatches(package.ItemNameToId, query);

            if (matches.Count == 0)
            {
                await SendOutputAsync(connectionId, $"Command rejected: item `{query}` not found.");
                return;
            }
            if (matches.Count > 1)
            {
                var multi = string.Join(Environment.NewLine, matches.Select(m => " - " + m.Key));
                await SendOutputAsync(connectionId, "Multiple matches found:" + Environment.NewLine + multi);
                return;
            }

            var itemName = matches.Single().Key;
            var itemId = matches.Single().Value;

            if (!_multiData.Locations.TryGetValue(slotId, out var locations))
            {
                await SendOutputAsync(connectionId, "Command rejected: catastrophic failure.");
                return;
            }

            var match = locations.FirstOrDefault(kvp => kvp.Value.Length >= 3 && kvp.Value[0] == itemId);
            if (match.Key == 0 && match.Value == null)
            {
                await SendOutputAsync(connectionId, $"Command rejected: No location contains item '{itemName}' (id {itemId}) for slot {slotId}.");
                return;
            }

            var locId = match.Key;
            var data = match.Value;

            var lookedUpItemId = data[0];
            var receivingPlayer = data[1];
            var itemFlags = (NetworkItemFlags)data[2];
            var status = itemFlags.HasFlag(NetworkItemFlags.Trap) ? HintStatus.Avoid : itemFlags == NetworkItemFlags.Advancement ? HintStatus.Priority : HintStatus.Unspecified;

            var locationName = package.LocationNameToId.FirstOrDefault(kvp => kvp.Value == locId).Key ?? locId.ToString();

            var existingHints = await _hintService.GetHintsAsync(slotId);
            var existing = existingHints.FirstOrDefault(h => h.Location == locId && h.Item == itemId && h.ReceivingPlayer == receivingPlayer);
            if (existing is not null)
            {
                var statusExisting = await _hintService.GetHintStatusAsync(slotId, existing);
                await SendOutputAsync(connectionId, $"Command Rejected: Hint already exists for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, receiving player {receivingPlayer}), status {statusExisting}.");
                return;
            }

            var hint = new Hint(receivingPlayer, slotId, locId, lookedUpItemId, found: false, entrance: string.Empty, itemFlags: itemFlags);
            await _hintService.AddHintAsync(slotId, hint, status);

            await NotifySubscribersAsync(slotId, _hintService, _storage, ConnectionManager);

            await SendOutputAsync(connectionId, $"Hint created for slot {slotId}: item '{itemName}' (id {itemId}) at location '{locationName}' (id {locId}, receiving player {receivingPlayer}).");
        }

        private static async Task NotifySubscribersAsync(long slotId, IHintService hintService, IStorageService storage, WebSocketConnectionManager connectionManager)
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

        private async Task SendOutputAsync(string connectionId, string output)
        {
            var print = new PrintJson(new JsonMessagePart.Text[] { new JsonMessagePart.Text(output) });
            await SendToConnectionAsync(connectionId, print);
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

        private class ServerConfig { public string? IngameCommandPrefix { get; set; } }
    }
}
