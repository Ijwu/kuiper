using kbo.littlerocks;

namespace kuiper.Core.Services.Abstract
{
    public interface ILocationCheckService
    {
        Task<NetworkItem?> AddCheckAsync(long slot, long locationId);
        Task<IEnumerable<long>> GetChecksAsync(long slot);
        Task<bool> HasCheckAsync(long slot, long locationId);
    }
}
