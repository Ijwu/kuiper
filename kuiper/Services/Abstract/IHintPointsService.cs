namespace kuiper.Services.Abstract
{
    public interface IHintPointsService
    {
        Task AddHintPointsAsync(long slot, int points);
        Task<int> GetHintPointsAsync(long slot);
        Task SetHintPointsAsync(long slot, int points);
    }
}