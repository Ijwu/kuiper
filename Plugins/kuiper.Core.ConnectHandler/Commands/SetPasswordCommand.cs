using kuiper.Commands.Abstract;
using kuiper.Core.Pickle;
using kuiper.Core.Extensions;
using kuiper.Core.Services.Abstract;
using kuiper.Core.Constants;

namespace kuiper.Core.ConnectHandler.Commands
{
    public class SetPasswordCommand : ICommand
    {
        private readonly MultiData _multiData;
        private readonly IStorageService _storageService;

        public SetPasswordCommand(MultiData multiData, IStorageService storageService)
        {
            _multiData = multiData;
            _storageService = storageService;
        }

        public string Name => "password";
        public string Description => "Set password for a slot: password <slotId_or_name> <password>";

        public async Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken cancellationToken)
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
            
            await _storageService.SaveAsync(StorageKeys.Password(slotId!.Value), password);
            
            return $"Password set for slot {slotId} ({_multiData.SlotInfo[(int)slotId].Name}).";
        }
    }
}
