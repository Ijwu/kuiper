using kuiper.Commands.Abstract;
using kuiper.Core.Pickle;
using kuiper.Core.Extensions;
using kuiper.Core.Services.Abstract;
using kuiper.Core.Constants;

namespace kuiper.Core.Connections.Commands
{
    public class SetPasswordCommand : ICommand
    {
        private readonly MultiData _multiData;
        private readonly INotifyingStorageService _storageService;

        public SetPasswordCommand(MultiData multiData, INotifyingStorageService storageService)
        {
            _multiData = multiData;
            _storageService = storageService;
        }

        public string Name => "password";
        public string Description => "Set password for a slot: password <slotId_or_name> <password>";

        public async Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken cancellationToken)
        {
            if (executingSlot == -1)
            {
                return await ServerConsoleInternal(args);
            }
            else
            {
                return await PlayerCommandInternal(args, executingSlot);
            }
            
        }

        private async Task<string> PlayerCommandInternal(string[] args, long executingSlot)
        {
            var password = string.Join(" ", args);
            if (string.IsNullOrEmpty(password))
            {
                await _storageService.DeleteAsync(StorageKeys.Password(executingSlot), -1);
                return "Slot password removed.";
            }
            await _storageService.SaveAsync(StorageKeys.Password(executingSlot), password, -1);
            return "Slot password has been set.";
        }

        private async Task<string> ServerConsoleInternal(string[] args)
        {
            if (args.Length < 2)
            {
                return "Usage: password <slotId_or_name> <password>";
            }

            var identifier = args[0];
            var password = args[1];

            if (!_multiData.TryResolveSlotId(identifier, out var slotId))
            {
                return $"Slot '{identifier}' not found.";
            }

            await _storageService.SaveAsync(StorageKeys.Password(slotId!.Value), password, -1);

            return $"Password set for slot {slotId} ({_multiData.SlotInfo[(int)slotId].Name}).";
        }
    }
}
