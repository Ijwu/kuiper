using kbo;
using kbo.littlerocks;

namespace kuiper.Services.Abstract
{
    public interface IHintService
    {
        Task<Hint[]> GetHintsAsync(long slotId);
        Task<HintStatus> GetHintStatusAsync(long slotId, Hint hint);

        Task AddHintAsync(long slotId, Hint hint, HintStatus status);

        Task UpdateHintAsync(long slotId, Hint hint, HintStatus status);
    }
}
