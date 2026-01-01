using Microsoft.Extensions.Hosting;

namespace kuiper.Commands
{
    public class QuitCommand : IConsoleCommand
    {
        public string Name => "quit";

        public string Description => "Shut down the server";

        public Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            var lifetime = services.GetRequiredService<IHostApplicationLifetime>();
            lifetime.StopApplication();
            return Task.CompletedTask;
        }
    }
}
