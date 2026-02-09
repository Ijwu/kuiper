namespace kuiper.Core.Services.Abstract
{
    public interface IHintPointsService
    {
        Task AddHintPointsAsync(long slot, long points);
        Task<long> GetHintPointsAsync(long slot);
    }
}