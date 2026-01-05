using Microsoft.Extensions.DependencyInjection;
using kuiper.Services;
using kuiper.Pickle;
using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class ListSlotsCommand : IConsoleCommand
    {
        public string Name => "listslots";
        public string Description => "List currently connected players and their mapped slots";

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            var connectionManager = services.GetRequiredService<WebSocketConnectionManager>();
            var multiData = services.GetRequiredService<MultiData>();
            var storage = services.GetRequiredService<IStorageService>();

            var connections = connectionManager.GetAllConnectionIds();
            if (connections.Count == 0)
            {
                return "No active connections.";
            }

            const string tagKeyPrefix = "#connection_tags:";
            var lines = new List<string>();
            foreach (var connectionId in connections.OrderBy(id => id))
            {
                var slot = await connectionManager.GetSlotForConnectionAsync(connectionId);
                string slotLabel = slot.HasValue ? slot.Value.ToString() : "(unmapped)";
                string slotName = string.Empty;
                string game = string.Empty;
                if (slot.HasValue && multiData.SlotInfo.TryGetValue((int)slot.Value, out var info))
                {
                    slotName = string.IsNullOrWhiteSpace(info.Name) ? string.Empty : $" ({info.Name})";
                    game = string.IsNullOrWhiteSpace(info.Game) ? string.Empty : $" - {info.Game}";
                }

                var tags = await storage.LoadAsync<string[]>(tagKeyPrefix + connectionId) ?? Array.Empty<string>();
                var tagText = tags.Length > 0 ? $" tags: [{string.Join(", ", tags)}]" : string.Empty;

                lines.Add($"{connectionId} => slot {slotLabel}{slotName}{game}{tagText}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
