using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class PrecollectedItemSeeder : IPrecollectedItemSeeder
    {
        private readonly ILogger<PrecollectedItemSeeder> _logger;
        private readonly MultiData _multiData;
        private readonly IReceivedItemService _receivedItemService;

        public PrecollectedItemSeeder(
            ILogger<PrecollectedItemSeeder> logger,
            MultiData multiData,
            IReceivedItemService receivedItemService)
        {
            _logger = logger;
            _multiData = multiData;
            _receivedItemService = receivedItemService;
        }

        public async Task SeedAsync()
        {
            var precollectedItems = _multiData.PrecollectedItems ?? [];
            const long precollectedSenderId = 0;
            const long precollectedLocationId = -2;

            var totalSlots = precollectedItems.Count;
            var totalItems = 0;

            foreach (var slotItems in precollectedItems)
            {
                var receivingSlot = slotItems.Key;
                var itemIds = slotItems.Value ?? [];

                foreach (var itemId in itemIds)
                {
                    var networkItem = new NetworkItem(itemId, precollectedLocationId, receivingSlot, NetworkItemFlags.None);
                    await _receivedItemService.AddReceivedItemAsync(receivingSlot, precollectedSenderId, networkItem);
                    totalItems++;

                    _logger.LogDebug(
                        "Seeded pre-collected item {ItemId} for slot {SlotId}.",
                        itemId,
                        receivingSlot);
                }
            }

            _logger.LogInformation(
                "Pre-collected item seeding complete. Slots: {SlotCount}, Items: {ItemCount}.",
                totalSlots,
                totalItems);
        }
    }
}
