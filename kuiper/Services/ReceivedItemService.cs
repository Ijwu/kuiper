using kbo.littlerocks;
using kuiper.Services.Abstract;

namespace kuiper.Services
{
    public class ReceivedItemService : IReceivedItemService
    {
        private readonly IStorageService _storage;
        private const string KeyPrefix = "#received:slot:";

        public ReceivedItemService(IStorageService storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        private static string KeyForSlot(long slotId) => KeyPrefix + slotId;

        public async Task AddReceivedItemAsync(long slot, NetworkItem item)
        {
            if (item == null) return;

            var key = KeyForSlot(slot);
            var existing = (await _storage.LoadAsync<NetworkItem[]>(key)) ?? Array.Empty<NetworkItem>();
            // avoid duplicates by location
            if (existing.Any(i => i.Location == item.Location) && item.Location != 0)
                return;

            var updated = existing.Concat(new[] { item }).ToArray();
            await _storage.SaveAsync(key, updated);
        }

        public async Task<IEnumerable<NetworkItem>> GetReceivedItemsAsync(long slot)
        {
            var key = KeyForSlot(slot);
            var items = await _storage.LoadAsync<NetworkItem[]>(key);
            return items ?? Array.Empty<NetworkItem>();
        }
    }
}
