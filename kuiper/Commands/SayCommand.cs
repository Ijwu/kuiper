using kuiper.Commands.Abstract;
using kuiper.Core.Services.Abstract;

namespace kuiper.Commands
{
    public class SayCommand : ICommand
    {
        private readonly IServerAnnouncementService _announcementService;

        public SayCommand(IServerAnnouncementService announcementService)
        {
            _announcementService = announcementService;
        }
        public string Name => "say";

        public string Description => "Broadcast a server message.";

        public async Task<string> ExecuteAsync(string[] args, long sendingSlot, CancellationToken cancellationToken)
        {
            if (sendingSlot != -1)
            {
                return "The 'say' command may only be used from the server console.";
            }

            if (args.Length == 0)
            {
                return "Usage: say <message>";
            }

            var message = string.Join(' ', args);
            if (!string.IsNullOrWhiteSpace(message))
            {
                await _announcementService.BroadcastServerMessageAsync(message);
                return "";
            }

            return "";
        }
    }
}
