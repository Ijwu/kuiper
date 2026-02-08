using System.Text;

using kuiper.Commands.Abstract;

namespace kuiper.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly ICommandRegistry _commandRegistry;

        public HelpCommand(ICommandRegistry commandRegistry)
        {
            _commandRegistry = commandRegistry;
        }

        public string Name => "help";

        public string Description => "List available commands";

        public Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Commands:");
            foreach (var cmd in _commandRegistry.ListCommands())
            {
                sb.AppendLine($"  {cmd.Name} - {cmd.Description}");
            }
            return Task.FromResult(sb.ToString().TrimEnd());
        }
    }
}
