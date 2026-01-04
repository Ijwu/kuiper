using kbo.littlerocks;

namespace kuiper.Services.Abstract
{
    public interface IReceivedItemService
    {
        Task AddReceivedItemAsync(long receivingSlot, long sendingSlot, NetworkItem item);
        Task<IEnumerable<(NetworkItem, long)>> GetReceivedItemsAsync(long slot);
    }
}
