namespace kuiper.Core.Services.Abstract
{
    public interface INotifyingStorageService
    {
        Task SaveAsync<T>(string key, T value, long settingSlot);
        Task<T?> LoadAsync<T>(string key);
        Task<bool> ExistsAsync(string key);
        Task DeleteAsync(string key, long settingSlot);
        Task<IEnumerable<string>> ListKeysAsync();
    }
}
