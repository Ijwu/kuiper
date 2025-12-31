using kuiper.Services.Abstract;

namespace kuiper.Services
{
    public class HintPointsService : IHintPointsService
    {
        private readonly IStorageService _storage;
        private const string KeyPrefix = "#hintpoints:slot:";

        public HintPointsService(IStorageService storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        private string KeyForSlot(long slot) => KeyPrefix + slot;

        public async Task AddHintPointsAsync(long slot, int points)
        {
            var current = await _storage.LoadAsync<int?>(KeyForSlot(slot)) ?? 0;
            var updated = current + points;
            await _storage.SaveAsync(KeyForSlot(slot), updated);
        }

        public async Task<int> GetHintPointsAsync(long slot)
        {
            return await _storage.LoadAsync<int?>(KeyForSlot(slot)) ?? 0;
        }

        public async Task SetHintPointsAsync(long slot, int points)
        {
            await _storage.SaveAsync(KeyForSlot(slot), points);
        }
    }
}
