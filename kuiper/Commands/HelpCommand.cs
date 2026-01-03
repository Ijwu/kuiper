using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace kuiper.Commands
{
    public class HelpCommand : IConsoleCommand
    {
        public string Name => "help";

        public string Description => "List available commands";

        public Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken)
        {
            var registry = services.GetRequiredService<CommandRegistry>();
            var sb = new StringBuilder();
            sb.AppendLine("Commands:");
            foreach (var cmd in registry.List())
            {
                sb.AppendLine($"  {cmd.Name} - {cmd.Description}");
            }
            Console.WriteLine(sb.ToString());
            return Task.CompletedTask;
        }
    }
}
