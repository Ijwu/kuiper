using kuiper.Core.Services.Abstract;
using kuiper.Core.Pickle;
using kuiper.Commands.Abstract;
using kuiper.Core.Constants;

namespace kuiper.Commands
{
    public class ListSlotsCommand : ICommand
    {
        private readonly IConnectionManager _connectionManager;
        private readonly MultiData _multiData;
        private readonly INotifyingStorageService _storageService;

        public ListSlotsCommand(IConnectionManager connectionManager, MultiData multiData, INotifyingStorageService storageService)
        {
            _connectionManager = connectionManager;
            _multiData = multiData;
            _storageService = storageService;
        }
        public string Name => "listslots";
        public string Description => "List currently connected players and their mapped slots.";

        public async Task<string> ExecuteAsync(string[] args, long sendingSlot, CancellationToken cancellationToken)
        {
            var connections = await _connectionManager.GetAllConnectionIdsAsync();
            if (!connections.Any())
            {
                return "No active connections.";
            }

            var lines = new List<string>();
            foreach (var connectionId in connections.OrderBy(id => id))
            {
                long? slot = await _connectionManager.GetSlotForConnectionAsync(connectionId);

                string slotLabel = slot.HasValue ? slot.Value.ToString() : "(unmapped)";
                string slotName = string.Empty;
                string game = string.Empty;

                if (slot.HasValue && _multiData.SlotInfo.TryGetValue((int)slot.Value, out MultiDataNetworkSlot? info))
                {
                    slotName = string.IsNullOrWhiteSpace(info.Name) ? string.Empty : $" ({info.Name})";
                    game = string.IsNullOrWhiteSpace(info.Game) ? string.Empty : $" - {info.Game}";
                }

                string[] tags = await _storageService.LoadAsync<string[]>(StorageKeys.ConnectionTags(connectionId)) ?? [];
                string tagText = tags.Length > 0 ? $" tags: [{string.Join(", ", tags)}]" : string.Empty;

                lines.Add($"{connectionId} => slot {slotLabel}{slotName}{game}{tagText}");
            }

            return string.Join(Environment.NewLine, lines);
        }
    }
}
