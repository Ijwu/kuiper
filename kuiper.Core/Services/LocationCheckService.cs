using kbo.littlerocks;

using kuiper.Core.Constants;
using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class LocationCheckService : ILocationCheckService
    {
        private readonly ILogger<LocationCheckService> _logger;
        private readonly MultiData _multiData;
        private readonly IStorageService _storageService;
        private readonly IReceivedItemService _receivedItemService;

        public LocationCheckService(ILogger<LocationCheckService> logger, MultiData multiData, IStorageService storageService, IReceivedItemService receivedItemService)
        {
            _logger = logger;
            _multiData = multiData;
            _storageService = storageService;
            _receivedItemService = receivedItemService;
        }

        public async Task<NetworkItem?> AddCheckAsync(long slot, long locationId)
        {
            var slotKey = StorageKeys.Checks(slot);

            var checks = await _storageService.LoadAsync<long[]>(slotKey) ?? Array.Empty<long>();
            if (!checks.Contains(locationId))
            {
                long[] newChecks = [..checks, locationId];
                await _storageService.SaveAsync(slotKey, newChecks);

                // Get item info for location from multidata
                var itemData = _multiData.Locations[slot][locationId];

                var item = new NetworkItem(itemData[0], locationId, itemData[1], (NetworkItemFlags)itemData[2]);

                // Record received item for the receiving player
                await _receivedItemService.AddReceivedItemAsync(item.Player, slot, item);

                _logger.LogDebug("Location ({LocationId}) for slot ({Slot}) has been checked.", locationId, slot);

                return item;
            }

            return null;
        }

        public async Task<IEnumerable<long>> GetChecksAsync(long slot)
        {
            return await _storageService.LoadAsync<long[]>(StorageKeys.Checks(slot)) ?? Array.Empty<long>();
        }

        public async Task<bool> HasCheckAsync(long slot, long locationId)
        {
            return (await GetChecksAsync(slot)).Contains(locationId);
        }
    }
}
