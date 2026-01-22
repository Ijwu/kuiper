using kuiper.Services.Abstract;
using kuiper.Pickle;
using kuiper.Utilities;

namespace kuiper.Commands
{
    public class SetPasswordCommand : IConsoleCommand
    {
        public string Name => "password";
        public string Description => "Set password for a slot: password <slotId_or_name> <password>";

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length < 2)
            {
                return "Usage: password <slotId_or_name> <password>";
            }

            var identifier = args[0];
            var password = args[1];

            var multiData = services.GetRequiredService<MultiData>();
            if (!SlotResolver.TryResolveSlotId(identifier, multiData, out var slotId))
            {
                return $"Slot '{identifier}' not found.";
            }
            
            var storage = services.GetRequiredService<IStorageService>();
            await storage.SaveAsync($"#password:slot:{slotId}", password);
            
            return $"Password set for slot {slotId} ({multiData.SlotInfo[(int)slotId].Name}).";
        }
    }
}
