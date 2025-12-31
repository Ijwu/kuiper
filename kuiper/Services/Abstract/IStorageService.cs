namespace kuiper.Services.Abstract
{
    public interface IStorageService
    {
        Task SaveAsync<T>(string key, T value);
        Task<T?> LoadAsync<T>(string key);
        Task<bool> ExistsAsync(string key);
        Task DeleteAsync(string key);
        Task<IEnumerable<string>> ListKeysAsync();
    }
}