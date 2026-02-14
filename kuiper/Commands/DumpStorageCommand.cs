using System.Text;
using System.Text.Json;

using kuiper.Commands.Abstract;
using kuiper.Core.Services.Abstract;

namespace kuiper.Commands
{
    public class DumpStorageCommand : ICommand
    {
        private readonly INotifyingStorageService _storage;

        public DumpStorageCommand(INotifyingStorageService storage)
        {
            _storage = storage;
        }

        public string Name => "dumpkey";

        public string Description => "Pretty print the current value of a data storage key.";

        public async Task<string> ExecuteAsync(string[] args, long sendingSlot, CancellationToken cancellationToken)
        {
            if (args.Length == 0)
            {
                string[] keysInStorage = (await _storage.ListKeysAsync())?.ToArray() ?? [];
                
                if (keysInStorage.Length == 0)
                {
                    return "No keys found.";
                }

                StringBuilder sb = new();
                sb.AppendLine("Keys:");

                foreach (var k in keysInStorage)
                {
                    sb.AppendLine(" - " + k);
                }

                return sb.ToString().TrimEnd();
            }

            var key = args[0];
            var value = await _storage.LoadAsync<object>(key);
            if (value is null)
            {
                return $"Key '{key}' not found or null.";
            }

            try
            {
                string json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
                return json;
            }
            catch
            {
                return value?.ToString() ?? "(null)";
            }
        }
    }
}
