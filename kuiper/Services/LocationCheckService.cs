using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services.Abstract;

namespace kuiper.Services
{
    public class LocationCheckService : ILocationCheckService
    {
        private readonly IStorageService _storage;
        private readonly MultiData _multiData;
        private readonly IReceivedItemService _receivedItems;

        public LocationCheckService(IStorageService storage, MultiData multiData, IReceivedItemService receivedItems)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _receivedItems = receivedItems ?? throw new ArgumentNullException(nameof(receivedItems));
        }

        private string KeyForSlot(long slot) => $"#checks:slot:{slot}";

        public async Task<NetworkItem?> AddCheckAsync(long slot, long locationId)
        {
            var key = KeyForSlot(slot);

            var checks = await _storage.LoadAsync<long[]>(key) ?? Array.Empty<long>();
            if (!checks.Contains(locationId))
            {
                var newChecks = checks.Concat(new[] { locationId }).ToArray();
                await _storage.SaveAsync(key, newChecks);

                // Get item info for location from multidata
                var itemData = _multiData.Locations[slot][locationId];

                var item = new NetworkItem(itemData[0], locationId, itemData[1], (NetworkItemFlags)itemData[2]);

                // Record received item for the receiving player
                await _receivedItems.AddReceivedItemAsync(item.Player, slot, item);

                return item;
            }

            return null; // no new item
        }

        public async Task<IEnumerable<long>> GetChecksAsync(long slot)
        {
            var key = KeyForSlot(slot);
            var checks = await _storage.LoadAsync<long[]>(key);
            return checks ?? Array.Empty<long>();
        }

        public async Task RemoveCheckAsync(long slot, long locationId)
        {
            var key = KeyForSlot(slot);
            
            var checks = await _storage.LoadAsync<long[]>(key) ?? Array.Empty<long>();
            if (checks.Contains(locationId))
            {
                var newChecks = checks.Where(x => x != locationId).ToArray();
                await _storage.SaveAsync(key, newChecks);
            }
        }

        public async Task ClearChecksAsync(long slot)
        {
            var key = KeyForSlot(slot);
            await _storage.DeleteAsync(key);
        }

        public async Task<bool> HasCheckAsync(long slot, long locationId)
        {
            var key = KeyForSlot(slot);
            var checks = await _storage.LoadAsync<long[]>(key);
            return (checks ?? Array.Empty<long>()).Contains(locationId);
        }
    }
}
