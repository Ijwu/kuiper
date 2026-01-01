using System.Collections.Concurrent;

namespace kuiper.Commands
{
    public class CommandRegistry
    {
        private readonly ConcurrentDictionary<string, IConsoleCommand> _commands = new(StringComparer.OrdinalIgnoreCase);

        public void Register(IConsoleCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _commands[command.Name] = command;
        }

        public bool TryGet(string name, out IConsoleCommand command)
        {
            return _commands.TryGetValue(name, out command!);
        }

        public IEnumerable<IConsoleCommand> List() => _commands.Values.OrderBy(c => c.Name);
    }
}
