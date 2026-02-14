
using kuiper.Commands.Abstract;
using kuiper.Core.Constants;
using kuiper.Core.Extensions;
using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;

namespace kuiper.Commands
{
    public class AuthorizeSlotCommand : ICommand
    {
        private readonly MultiData _multiData;
        private readonly INotifyingStorageService _storageService;

        public AuthorizeSlotCommand(MultiData multiData, INotifyingStorageService storageService)
        {
            _multiData = multiData;
            _storageService = storageService;
        }

        public string Name => "authslot";

        public string Description => "Authorize a slot to run in-game commands.";

        public async Task<string> ExecuteAsync(string[] args, long sendingSlot, CancellationToken cancellationToken)
        {
            if (sendingSlot == -1)
            {
                return await ServerAuthSlotInternalAsync(args, sendingSlot);
            }
            else
            {
                return await PlayerAuthSlotInternalAsync(args, sendingSlot);
            }
        }

        //TODO: implement the authslot command for when a player runs it
        private async Task<string> PlayerAuthSlotInternalAsync(string[] args, long sendingSlot)
        {
            return "Not implemented yet. Sorry. Use the console.";
        }

        private async Task<string> ServerAuthSlotInternalAsync(string[] args, long sendingSlot)
        {
            if (args.Length != 1)
            {
                return "Usage: authslot <slotId_or_name>";
            }

            var identifier = args[0];

            if (!_multiData.TryResolveSlotId(identifier, out var slotId))
            {
                return $"Slot '{identifier}' not found.";
            }

            long[] current = (await _storageService.LoadAsync<long[]>(StorageKeys.AuthorizedCommandSlots)) ?? [];
            if (current.Contains(slotId!.Value))
            {
                return $"Slot {slotId} is already authorized.";
            }

            if (!current.Contains(sendingSlot) && sendingSlot != -1)
            {
                return $"You are unauthorized to use this command.";
            }

            var updated = current.Concat([slotId!.Value]).Distinct().ToArray();
            await _storageService.SaveAsync(StorageKeys.AuthorizedCommandSlots, updated, -1);
            return $"Authorized slot {slotId}.";
        }
    }
}
