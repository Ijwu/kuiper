using kuiper.Commands.Abstract;
using kuiper.Core.Extensions;
using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

namespace kuiper.Core.Checks.Commands
{
    public class ReleaseCommand : ICommand
    {
        private readonly IReleaseService _releaseService;
        private readonly MultiData _multiData;

        public string Name => "release";
        public string Description => "Releases remaining items for a player slot. Usage: release <slot_id_or_name>";

        public ReleaseCommand(IReleaseService releaseService, MultiData multiData)
        {
            _releaseService = releaseService;
            _multiData = multiData;
        }

        //TODO: Release permissions, authorized slot permissions
        public async Task<string> ExecuteAsync(string[] args, long sendingSlot, CancellationToken cancellationToken)
        {
            if (args.Length == 0)
                return "Usage: release <slot_id_or_name>";

            var identifier = string.Join(" ", args);
            
            if (!_multiData.TryResolveSlotId(identifier, out var slotId))
            {
                return $"Slot '{identifier}' not found.";
            }

             await _releaseService.ReleaseRemainingItemsAsync(slotId!.Value);
             return $"Released items for slot {slotId}.";
        }
    }
}
