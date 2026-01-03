using Microsoft.Extensions.DependencyInjection;
using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class AuthorizeCommandSlot : IConsoleCommand
    {
        public string Name => "authslot";

        public string Description => "Authorize a slot to run in-game commands: authslot <slotId>";

        private const string AuthorizedSlotsKey = "#authorized_command_slots";

        public async Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length != 1 || !long.TryParse(args[0], out var slotId))
            {
                Console.WriteLine("Usage: authslot <slotId>");
                return;
            }

            var storage = services.GetRequiredService<IStorageService>();
            var current = (await storage.LoadAsync<long[]>(AuthorizedSlotsKey)) ?? Array.Empty<long>();
            if (current.Contains(slotId))
            {
                Console.WriteLine($"Slot {slotId} is already authorized.");
                return;
            }

            var updated = current.Concat(new[] { slotId }).Distinct().ToArray();
            await storage.SaveAsync(AuthorizedSlotsKey, updated);
            Console.WriteLine($"Authorized slot {slotId}.");
        }
    }
}
