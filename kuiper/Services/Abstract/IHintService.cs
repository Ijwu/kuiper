using kbo;
using kbo.littlerocks;

namespace kuiper.Services.Abstract
{
    public interface IHintService
    {
        Task<Hint[]> GetHintsAsync(long slotId);

        Task AddHintAsync(long slotId, Hint hint);

        Task UpdateHintAsync(long slotId, Hint hint);

        Task<bool> HintExistsForLocationAsync(long location);
    }
}
