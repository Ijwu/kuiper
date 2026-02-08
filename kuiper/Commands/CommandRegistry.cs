using System.Collections.Concurrent;

using kuiper.Commands.Abstract;

namespace kuiper.Commands
{
    public class CommandRegistry : ICommandRegistry
    {
        private readonly ConcurrentDictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _commands[command.Name] = command;
        }

        public bool TryGetCommand(string name, out ICommand command)
        {
            return _commands.TryGetValue(name, out command!);
        }

        public IEnumerable<ICommand> ListCommands() => _commands.Values.OrderBy(c => c.Name);
    }
}
