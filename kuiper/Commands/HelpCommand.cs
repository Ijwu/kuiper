using System.Text;

namespace kuiper.Commands
{
    public class HelpCommand : IConsoleCommand
    {
        private readonly CommandRegistry _registry;

        public HelpCommand(CommandRegistry registry)
        {
            _registry = registry;
        }

        public string Name => "help";

        public string Description => "List available commands";

        public Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Commands:");
            foreach (var cmd in _registry.List())
            {
                sb.AppendLine($"  {cmd.Name} - {cmd.Description}");
            }
            Console.WriteLine(sb.ToString());
            return Task.CompletedTask;
        }
    }
}
