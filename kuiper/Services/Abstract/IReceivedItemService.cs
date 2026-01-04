using kbo.littlerocks;

namespace kuiper.Services.Abstract
{
    public interface IReceivedItemService
    {
        Task AddReceivedItemAsync(long slot, NetworkItem item);
        Task<IEnumerable<NetworkItem>> GetReceivedItemsAsync(long slot);
    }
}
