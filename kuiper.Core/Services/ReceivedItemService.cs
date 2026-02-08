using kbo.littlerocks;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class ReceivedItemService : IReceivedItemService
    {
        private record StoredReceivedItem(long Sender, NetworkItem Item);

        private readonly ILogger<ReceivedItemService> _logger;
        private readonly IStorageService _storageService;

        public ReceivedItemService(ILogger<ReceivedItemService> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        public async Task AddReceivedItemAsync(long receivingSlot, long sendingSlot, NetworkItem item)
        {
            var key = StorageKeys.ReceivedItems(receivingSlot);
            var existing = await _storageService.LoadAsync<StoredReceivedItem[]>(key) ?? Array.Empty<StoredReceivedItem>();

            // Skip dupes by item
            if (existing.Any(x => x.Item == item))
            {
                _logger.LogDebug("Item was marked as received but it was a duplicate. Receiver: ({ReceivingSlot}) Sender: ({SendingSlot}) Item: ({Item})", receivingSlot, sendingSlot, item);
                return;
            }

            await _storageService.SaveAsync<StoredReceivedItem[]>(key, [.. existing, new StoredReceivedItem(sendingSlot, item)]);
            _logger.LogDebug("Item ({Item}) was received by slot ({ReceivingSlot}) from sending slot ({SendingSlot}).", item, receivingSlot, sendingSlot);
        }

        public async Task<IEnumerable<(NetworkItem, long)>> GetReceivedItemsAsync(long slot)
        {
            return (await _storageService.LoadAsync<StoredReceivedItem[]>(StorageKeys.ReceivedItems(slot)))
                   ?.Select(x => (x.Item, x.Sender))
                   ?? [];
        }
    }
}
