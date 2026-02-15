using System.Text.Json;

using kuiper.Commands.Abstract;
using kuiper.Core.Services.Abstract;

namespace kuiper.Commands
{
    public class RestoreStorageCommand : ICommand
    {
        private readonly INotifyingStorageService _storageService;

        public RestoreStorageCommand(INotifyingStorageService storageService)
        {
            _storageService = storageService;
        }
        public string Name => "restorestorage";
        public string Description => "Restore storage from a JSON backup.";

        public async Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken cancellationToken)
        {
            if (executingSlot != -1)
            {
                return "The 'restorestorage' command may only be used from the server console.";
            }

            if (args.Length != 1)
            {
                return "Usage: restorestorage <path>";
            }

            string path = args[0];
            if (!File.Exists(path))
            {
                return $"File not found: {path}";
            }

            string json = await File.ReadAllTextAsync(path, cancellationToken);
            List<Entry> entries = JsonSerializer.Deserialize<List<Entry>>(json) ?? new();

            int restored = 0;
            foreach (Entry entry in entries)
            {
                object? value = null;
                Type? type = null;
                if (!string.IsNullOrWhiteSpace(entry.Type))
                {
                    type = Type.GetType(entry.Type, throwOnError: false, ignoreCase: true);
                }

                if (type != null)
                {
                    value = JsonSerializer.Deserialize(entry.Json, type);
                    await InvokeSaveAsync(_storageService, type, entry.Key, value);
                }
                else
                {
                    value = JsonSerializer.Deserialize<object>(entry.Json);
                    await _storageService.SaveAsync(entry.Key, value!, -1);
                }
                restored++;
            }

            return $"Restored {restored} keys from {path}.";
        }

        private static async Task InvokeSaveAsync(INotifyingStorageService storage, Type type, string key, object? value)
        {
            var method = storage.GetType().GetMethod("SaveAsync");
            if (method == null) throw new InvalidOperationException("SaveAsync not found on storage service.");
            var generic = method.MakeGenericMethod(type);
            var task = (Task)generic.Invoke(storage, new object?[] { key, value!, -1 })!;
            await task.ConfigureAwait(false);
        }

        private record Entry(string Key, string Type, string Json);
    }
}
