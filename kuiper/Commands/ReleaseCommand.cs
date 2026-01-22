using kuiper.Services.Abstract;
using kuiper.Pickle;
using kuiper.Utilities; // Add this

namespace kuiper.Commands
{
    public class ReleaseCommand : IConsoleCommand
    {
        private readonly IReleaseService _releaseService;
        private readonly MultiData _multiData;

        public string Name => "release";
        public string Description => "Releases remaining items for a player slot. Usage: release <slot_id_or_name>";

        public ReleaseCommand(IReleaseService releaseService, MultiData multiData)
        {
            _releaseService = releaseService ?? throw new ArgumentNullException(nameof(releaseService));
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length == 0)
                return "Usage: release <slot_id_or_name>";

            var identifier = string.Join(" ", args);
            
            if (!SlotResolver.TryResolveSlotId(identifier, _multiData, out var slotId))
            {
                return $"Slot '{identifier}' not found.";
            }

             await _releaseService.ReleaseRemainingItemsAsync(slotId);
             return $"Released items for slot {slotId}.";
        }
    }
}
