using System.Text.Json;
using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class DumpStorageCommand : IConsoleCommand
    {
        public string Name => "dumpkey";

        public string Description => "Pretty print the current value of a data storage key: dumpkey <key>";

        public async Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dumpkey <key>");
                return;
            }

            var key = args[0];
            var storage = services.GetRequiredService<IStorageService>();
            var value = await storage.LoadAsync<object>(key);
            if (value is null)
            {
                Console.WriteLine($"Key '{key}' not found or null.");
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(json);
            }
            catch
            {
                Console.WriteLine(value?.ToString() ?? "(null)");
            }
        }
    }
}
