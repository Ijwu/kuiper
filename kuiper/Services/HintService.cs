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
            var stored = await _storage.LoadAsync<StoredHint[]>(KeyForSlot(slotId));
            return stored?.Select(h => h.Hint).ToArray() ?? Array.Empty<Hint>();
        }

        public async Task<HintStatus> GetHintStatusAsync(long slotId, Hint hint)
        {
            if (hint == null) return HintStatus.Unspecified;

            var stored = await _storage.LoadAsync<StoredHint[]>(KeyForSlot(slotId)) ?? Array.Empty<StoredHint>();
            var match = stored.FirstOrDefault(h => Matches(h.Hint, hint));
            return match?.Status ?? HintStatus.Unspecified;
        }

        public async Task AddHintAsync(long slotId, Hint hint, HintStatus status)
        {
            if (hint == null) return;

            var list = (await _storage.LoadAsync<StoredHint[]>(KeyForSlot(slotId)) ?? Array.Empty<StoredHint>()).ToList();
            var existingIndex = list.FindIndex(h => Matches(h.Hint, hint));
            var newEntry = new StoredHint(hint, status);

            if (existingIndex >= 0)
            {
                list[existingIndex] = newEntry;
            }
            else
            {
                list.Add(newEntry);
            }

            await _storage.SaveAsync(KeyForSlot(slotId), list.ToArray());
        }

        public async Task UpdateHintAsync(long slotId, Hint hint, HintStatus status)
        {
            // identical behavior to AddHintAsync but kept for interface symmetry
            await AddHintAsync(slotId, hint, status);
        }

        private static bool Matches(Hint a, Hint b) =>
            a.ReceivingPlayer == b.ReceivingPlayer &&
            a.FindingPlayer == b.FindingPlayer &&
            a.Location == b.Location;

        public record StoredHint
        {
            public Hint Hint { get; init; }
            public HintStatus Status { get; init; }

            public StoredHint(Hint hint, HintStatus status)
            {
                Hint = hint;
                Status = status;
            }
        }
    }
}
