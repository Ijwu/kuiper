using kuiper.Services.Abstract;
using kbo;
using kbo.littlerocks;

namespace kuiper.Services
{
    public class HintService : IHintService
    {
        private readonly IStorageService _storage;
        private const string KeyPrefix = "#hints:slot:";

        public HintService(IStorageService storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        private static string KeyForSlot(long slotId) => KeyPrefix + slotId;

        public async Task<Hint[]> GetHintsAsync(long slotId)
        {
            var stored = await _storage.LoadAsync<Hint[]>(KeyForSlot(slotId));
            return stored?.ToArray() ?? Array.Empty<Hint>();
        }

        public async Task AddHintAsync(long slotId, Hint hint)
        {
            if (hint == null) return;

            var list = (await _storage.LoadAsync<Hint[]>(KeyForSlot(slotId)) ?? Array.Empty<Hint>()).ToList();
            var existingIndex = list.FindIndex(h => Matches(h, hint));

            if (existingIndex >= 0)
            {
                list[existingIndex] = hint;
            }
            else
            {
                list.Add(hint);
            }

            await _storage.SaveAsync(KeyForSlot(slotId), list.ToArray());
        }

        public async Task UpdateHintAsync(long slotId, Hint hint)
        {
            // identical behavior to AddHintAsync but kept for interface symmetry
            await AddHintAsync(slotId, hint);
        }

        private static bool Matches(Hint a, Hint b) =>
            a.ReceivingPlayer == b.ReceivingPlayer &&
            a.FindingPlayer == b.FindingPlayer &&
            a.Location == b.Location;

        public async Task<bool> HintExistsForLocationAsync(long location)
        {
            var keys = await _storage.ListKeysAsync();
            var hintKeys = keys.Where(k => k.StartsWith(KeyPrefix));

            foreach (var key in hintKeys)
            {
                var stored = await _storage.LoadAsync<Hint[]>(key);
                if (stored != null && stored.Any(h => h.Location == location))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
