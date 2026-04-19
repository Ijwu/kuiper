using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Services
{
    public class PrecollectedHintSeeder : IPrecollectedHintSeeder
    {
        private readonly ILogger<PrecollectedHintSeeder> _logger;
        private readonly MultiData _multiData;
        private readonly IHintService _hintService;

        public PrecollectedHintSeeder(
            ILogger<PrecollectedHintSeeder> logger,
            MultiData multiData,
            IHintService hintService)
        {
            _logger = logger;
            _multiData = multiData;
            _hintService = hintService;
        }

        public async Task SeedAsync()
        {
            var precollectedHints = _multiData.PrecollectedHints ?? [];
            var totalSlots = precollectedHints.Count;
            var totalHints = 0;

            foreach (var slotHints in precollectedHints)
            {
                var slotId = slotHints.Key;
                var hints = slotHints.Value ?? [];
                foreach (var multiDataHint in hints)
                {
                    var hint = new Hint(
                        multiDataHint.ReceivingPlayer,
                        multiDataHint.FindingPlayer,
                        multiDataHint.Location,
                        multiDataHint.Item,
                        multiDataHint.Found,
                        multiDataHint.Entrance ?? string.Empty,
                        (NetworkItemFlags)multiDataHint.ItemFlags,
                        (HintStatus)multiDataHint.Status);

                    await _hintService.AddOrUpdateHintAsync(slotId, hint);
                    totalHints++;

                    _logger.LogDebug(
                        "Seeded pre-collected hint for slot {SlotId}: receiving {ReceivingPlayer}, finding {FindingPlayer}, location {Location}.",
                        slotId,
                        multiDataHint.ReceivingPlayer,
                        multiDataHint.FindingPlayer,
                        multiDataHint.Location);
                }
            }

            _logger.LogInformation(
                "Pre-collected hint seeding complete. Slots: {SlotCount}, Hints: {HintCount}.",
                totalSlots,
                totalHints);
        }
    }
}
