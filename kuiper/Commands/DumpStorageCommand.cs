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
            var storage = services.GetRequiredService<IStorageService>();

            if (args.Length == 0)
            {
                var keys = await storage.ListKeysAsync();
                var list = keys?.ToArray() ?? Array.Empty<string>();
                if (list.Length == 0)
                {
                    Console.WriteLine("No keys found.");
                }
                else
                {
                    Console.WriteLine("Keys:");
                    foreach (var k in list)
                    {
                        Console.WriteLine(" - " + k);
                    }
                }
                return;
            }

            var key = args[0];
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
