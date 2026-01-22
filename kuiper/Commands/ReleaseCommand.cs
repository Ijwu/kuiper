using kuiper.Services.Abstract;
using kuiper.Pickle;

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
            long slotId = -1;

            if (int.TryParse(identifier, out var id))
            {
                if (_multiData.SlotInfo.ContainsKey(id))
                    slotId = id;
            }

            if (slotId == -1)
            {
                var slot = _multiData.SlotInfo.Values.FirstOrDefault(s => string.Equals(s.Name, identifier, StringComparison.OrdinalIgnoreCase));
                if (slot != null && _multiData.SlotInfo.Any(kvp => kvp.Value == slot)) // Double check if needed, but values don't have ID directly usually?? Wait, MultiDataNetworkSlot probably has ID implicitly by map Key? No.
                {
                    // Find key for value
                     slotId = _multiData.SlotInfo.FirstOrDefault(x => x.Value == slot).Key;
                }
            }

            if (slotId == -1)
                return $"Slot '{identifier}' not found.";

             await _releaseService.ReleaseRemainingItemsAsync(slotId);
             return $"Released items for slot {slotId}.";
        }
    }
}
