using System.Net.WebSockets;

namespace kuiper.Core.Services.Abstract
{
    public interface IConnectionManager
    {
        Task AddConnectionAsync(string connectionId, WebSocket socket);
        Task RemoveConnectionAsync(string connectionId);
        Task SendToConnectionAsync(string connectionId, string message);
        Task SendJsonToConnectionAsync<T>(string connectionId, T obj);
        Task MapConnectionToSlotAsync(string connectionId, long slot);
        Task<long?> GetSlotForConnectionAsync(string connectionId);
        Task<IReadOnlyCollection<string>> GetConnectionIdsForSlotAsync(long slot);
        Task UnmapConnectionAsync(string connectionId);
    }
}
