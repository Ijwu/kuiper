using kbo.littlerocks;
using kuiper.Services.Abstract;
using kuiper.Constants;

namespace kuiper.Services
{
    public class ReceivedItemService : IReceivedItemService
    {
        private readonly IStorageService _storage;

        public ReceivedItemService(IStorageService storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        private static string KeyForSlot(long slotId) => StorageKeys.ReceivedItems(slotId);

        public async Task AddReceivedItemAsync(long receivingSlot, long sendingSlot, NetworkItem item)
        {
            if (item == null) return;

            var key = KeyForSlot(receivingSlot);
            var existing = (await _storage.LoadAsync<StoredReceivedItem[]>(key)) ?? Array.Empty<StoredReceivedItem>();
            // avoid duplicates by location
            if (existing.Any(i => i.Item.Location == item.Location) && item.Location != 0)
                return;

            var updated = existing.Concat(new[] { new StoredReceivedItem(sendingSlot, item) }).ToArray();
            await _storage.SaveAsync(key, updated);
        }

        public async Task<IEnumerable<(NetworkItem, long)>> GetReceivedItemsAsync(long slot)
        {
            var key = KeyForSlot(slot);
            var items = await _storage.LoadAsync<StoredReceivedItem[]>(key);
            return items?.Select(i => (i.Item, i.SendingSlot)) ?? Array.Empty<(NetworkItem, long)>();
        }

        public record StoredReceivedItem
        {
            public long SendingSlot { get; init; }
            public NetworkItem Item { get; init; }
            public StoredReceivedItem(long sendingSlot, NetworkItem item)
            {
                SendingSlot = sendingSlot;
                Item = item;
            }
        }
    }
}
