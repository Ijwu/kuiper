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

            var path = args[0];

            var keys = await _storageService.ListKeysAsync();
            var list = new List<Entry>();

            foreach (var key in keys)
            {
                var value = await _storageService.LoadAsync<object>(key);
                var typeName = value?.GetType().AssemblyQualifiedName ?? string.Empty;
                var json = JsonSerializer.Serialize(value);
                list.Add(new Entry(key, typeName, json));
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
            var payload = JsonSerializer.Serialize(list, options);
            await File.WriteAllTextAsync(path, payload, cancellationToken);

            return $"Backed up {list.Count} keys to {path}.";
        }

        private record Entry(string Key, string Type, string Json);
    }
}
