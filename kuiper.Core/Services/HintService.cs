using kbo;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class HintService : IHintService
    {
        private readonly ILogger<HintService> _logger;
        private readonly IStorageService _storage;

        public HintService(ILogger<HintService> logger, IStorageService storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public async Task<Hint[]> GetHintsAsync(long slotId)
        {
            var stored = await _storage.LoadAsync<Hint[]>(StorageKeys.Hint(slotId));
            return stored ?? [];
        }

        public async Task AddOrUpdateHintAsync(long slotId, Hint hint)
        {
            if (hint == null) return;

            var list = (await _storage.LoadAsync<Hint[]>(StorageKeys.Hint(slotId)) ?? []).ToList();
            var existingIndex = list.FindIndex(h => Matches(h, hint));

            if (existingIndex >= 0)
            {
                list[existingIndex] = hint;
                _logger.LogDebug("Updated existing hint for slot ({SlotId}). Hint: ({Hint})", slotId, hint);
            }
            else
            {
                list.Add(hint);
                _logger.LogDebug("Added new hint for slot ({SlotId}). Hint: ({Hint})", slotId, hint);
            }

            await _storage.SaveAsync(StorageKeys.Hint(slotId), list.ToArray());
        }

        private static bool Matches(Hint a, Hint b) =>
            a.ReceivingPlayer == b.ReceivingPlayer &&
            a.FindingPlayer == b.FindingPlayer &&
            a.Location == b.Location;

        public async Task<bool> HintExistsForLocationAsync(long location, long slotId)
        {
            Hint[]? stored = await _storage.LoadAsync<Hint[]>(StorageKeys.Hint(slotId));

            return stored != null && stored.Any(h => h.Location == location);
        }
    }
}
