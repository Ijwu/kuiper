using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class SayCommand : IConsoleCommand
    {
        public string Name => "say";

        public string Description => "Broadcast a server message: say <message>";

        public async Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length == 0)
            {
                return "Usage: say <message>";
            }

            var announcer = services.GetRequiredService<IServerAnnouncementService>();
            var message = string.Join(' ', args);
            if (!string.IsNullOrWhiteSpace(message))
            {
                await announcer.BroadcastServerMessageAsync(message);
                return "Broadcast sent.";
            }

            return "";
        }
    }
}
