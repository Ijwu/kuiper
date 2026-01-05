using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class RestoreStorageCommand : IConsoleCommand
    {
        public string Name => "restorestorage";
        public string Description => "Restore storage from a JSON backup: restorestorage <path>";

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length != 1)
            {
                return "Usage: restorestorage <path>";
            }

            var path = args[0];
            if (!File.Exists(path))
            {
                return $"File not found: {path}";
            }

            var storage = services.GetRequiredService<IStorageService>();
            var json = await File.ReadAllTextAsync(path, cancellationToken);
            var entries = JsonSerializer.Deserialize<List<Entry>>(json) ?? new();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            int restored = 0;
            foreach (var entry in entries)
            {
                try
                {
                    object? value = null;
                    Type? type = null;
                    if (!string.IsNullOrWhiteSpace(entry.Type))
                    {
                        type = Type.GetType(entry.Type, throwOnError: false, ignoreCase: true);
                    }

                    if (type != null)
                    {
                        value = JsonSerializer.Deserialize(entry.Json, type, options);
                        await InvokeSaveAsync(storage, type, entry.Key, value);
                    }
                    else
                    {
                        value = JsonSerializer.Deserialize<object>(entry.Json, options);
                        await storage.SaveAsync(entry.Key, value!);
                    }
                    restored++;
                }
                catch
                {
                    // skip problematic entries but continue
                }
            }

            return $"Restored {restored} keys from {path}.";
        }

        private static async Task InvokeSaveAsync(IStorageService storage, Type type, string key, object? value)
        {
            var method = storage.GetType().GetMethod("SaveAsync");
            if (method == null) throw new InvalidOperationException("SaveAsync not found on storage service.");
            var generic = method.MakeGenericMethod(type);
            var task = (Task)generic.Invoke(storage, new object?[] { key, value! })!;
            await task.ConfigureAwait(false);
        }

        private record Entry(string Key, string Type, string Json);
    }
}
