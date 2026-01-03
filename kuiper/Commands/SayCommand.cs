using Microsoft.Extensions.DependencyInjection;
using kuiper.Services.Abstract;

namespace kuiper.Commands
{
    public class SayCommand : IConsoleCommand
    {
        public string Name => "say";

        public string Description => "Broadcast a server message: say <message>";

        public async Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: say <message>");
                return;
            }

            var announcer = services.GetRequiredService<IServerAnnouncementService>();
            var message = string.Join(' ', args);
            if (!string.IsNullOrWhiteSpace(message))
            {
                await announcer.BroadcastServerMessageAsync(message);
            }
        }
    }
}
