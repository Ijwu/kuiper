using System.Text.Json;

using kuiper.Commands.Abstract;
using kuiper.Core.Services.Abstract;

namespace kuiper.Commands
{
    public class BackupStorageCommand : ICommand
    {
        private readonly INotifyingStorageService _storageService;

        public BackupStorageCommand(INotifyingStorageService storageService)
        {
            _storageService = storageService;
        }
        public string Name => "backupstorage";
        public string Description => "Back up all storage keys to a JSON file.";

        public async Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken cancellationToken)
        {
            if (executingSlot != -1)
            {
                return "The 'backupstorage' command may only be used from the server console.";
            }

            if (args.Length != 1)
            {
                return "Usage: backupstorage <path>";
            }

            string path = args[0];

            List<Entry> list = [];

            foreach (string key in await _storageService.ListKeysAsync())
            {
                object? value = await _storageService.LoadAsync<object>(key);
                string typeName = value?.GetType().AssemblyQualifiedName ?? string.Empty;
                string json = JsonSerializer.Serialize(value);
                list.Add(new Entry(key, typeName, json));
            }

            JsonSerializerOptions options = new() { WriteIndented = true };
            string directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".";
            Directory.CreateDirectory(directory);

            string payload = JsonSerializer.Serialize(list, options);
            await File.WriteAllTextAsync(path, payload, cancellationToken);

            return $"Backed up {list.Count} keys to {path}.";
        }

        private record Entry(string Key, string Type, string Json);
    }
}
