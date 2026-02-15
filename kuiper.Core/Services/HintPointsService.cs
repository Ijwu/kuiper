using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class HintPointsService : IHintPointsService
    {
        private readonly ILogger<HintPointsService> _logger;
        private readonly INotifyingStorageService _storageService;

        public HintPointsService(ILogger<HintPointsService> logger, INotifyingStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }
        public async Task AddHintPointsAsync(long slot, long points)
        {
            var hintPoints = await _storageService.LoadAsync<long>(StorageKeys.HintPoints(slot));

            await _storageService.SaveAsync(StorageKeys.HintPoints(slot), hintPoints + points, -1);

            _logger.LogDebug("Added `{HintPoints}` ({Total}) hint points to slot ({Slot}).", points, hintPoints + points, slot);
        }

        public async Task<long> GetHintPointsAsync(long slot)
        {
            return await _storageService.LoadAsync<long>(StorageKeys.HintPoints(slot));
        }
    }
}
