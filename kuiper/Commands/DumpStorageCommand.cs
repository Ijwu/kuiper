using System.Text.Json;

using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class DumpStorageCommand : IConsoleCommand
    {
        public string Name => "dumpkey";

        public string Description => "Pretty print the current value of a data storage key: dumpkey <key>";

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            var storage = services.GetRequiredService<IStorageService>();

            if (args.Length == 0)
            {
                var keys = await storage.ListKeysAsync();
                var list = keys?.ToArray() ?? Array.Empty<string>();
                if (list.Length == 0)
                {
                    return "No keys found.";
                }
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Keys:");
                foreach (var k in list)
                {
                    sb.AppendLine(" - " + k);
                }
                return sb.ToString().TrimEnd();
            }

            var key = args[0];
            var value = await storage.LoadAsync<object>(key);
            if (value is null)
            {
                return $"Key '{key}' not found or null.";
            }

            try
            {
                var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
                return json;
            }
            catch
            {
                return value?.ToString() ?? "(null)";
            }
        }
    }
}
