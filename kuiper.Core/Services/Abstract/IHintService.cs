using kbo.littlerocks;

namespace kuiper.Core.Services.Abstract
{
    public interface IHintService
    {
        Task<Hint[]> GetHintsAsync(long slotId);

        Task AddOrUpdateHintAsync(long slotId, Hint hint);

        Task<bool> HintExistsForLocationAsync(long location, long slotId);
    }
}
