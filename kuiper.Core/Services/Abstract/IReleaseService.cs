namespace kuiper.Core.Services.Abstract
{
    public interface IReleaseService
    {
        Task ReleaseRemainingItemsAsync(long slotId);
    }
}
