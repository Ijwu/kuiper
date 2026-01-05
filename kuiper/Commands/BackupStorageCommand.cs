using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class BackupStorageCommand : IConsoleCommand
    {
        public string Name => "backupstorage";
        public string Description => "Back up all storage keys to a JSON file: backupstorage <path>";

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length != 1)
            {
                return "Usage: backupstorage <path>";
            }

            var path = args[0];
            var storage = services.GetRequiredService<IStorageService>();
            var keys = await storage.ListKeysAsync();
            var list = new List<Entry>();

            foreach (var key in keys)
            {
                var value = await storage.LoadAsync<object>(key);
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
