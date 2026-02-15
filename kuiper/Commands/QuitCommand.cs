using kuiper.Commands.Abstract;

namespace kuiper.Commands
{
    public class QuitCommand : ICommand
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public QuitCommand(IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
        }
        public string Name => "quit";

        public string Description => "Shut down the server.";

        public Task<string> ExecuteAsync(string[] args, long sendingSlot, CancellationToken cancellationToken)
        {
            if (sendingSlot != -1)
            {
                return Task.FromResult("The 'quit' command may only be run from the server console.");
            }

            _hostApplicationLifetime.StopApplication();
            
            return Task.FromResult("");
        }
    }
}
