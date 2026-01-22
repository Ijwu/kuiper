namespace kuiper.Services.Abstract
{
    public interface IReleaseService
    {
        Task ReleaseRemainingItemsAsync(long slotId);
    }
}
