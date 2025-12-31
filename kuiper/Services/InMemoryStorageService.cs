using System.Collections.Concurrent;

using kuiper.Services.Abstract;

namespace kuiper.Services
{
    public class InMemoryStorageService : IStorageService
    {
        private readonly ConcurrentDictionary<string, object> _store = new();
        private static readonly object NullMarker = new();

        public InMemoryStorageService()
        {
        }

        public Task SaveAsync<T>(string key, T value)
        {
            // store the value directly; use a marker for null because ConcurrentDictionary disallows null values
            _store[key] = value is null ? NullMarker : (object)value!;
            return Task.CompletedTask;
        }

        public Task<T?> LoadAsync<T>(string key)
        {
            if (_store.TryGetValue(key, out var obj))
            {
                if (ReferenceEquals(obj, NullMarker))
                    return Task.FromResult<T?>(default);

                if (obj is T t)
                    return Task.FromResult<T?>(t);

                // stored value is not assignable to T
                return Task.FromResult<T?>(default);
            }

            return Task.FromResult<T?>(default);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_store.ContainsKey(key));
        }

        public Task DeleteAsync(string key)
        {
            _store.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> ListKeysAsync()
        {
            return Task.FromResult(_store.Keys.AsEnumerable());
        }
    }
}
