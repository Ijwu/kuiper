namespace kuiper.Commands.Abstract
{
    public interface ICommandRegistry
    {
        void RegisterCommand(ICommand command);
        bool TryGetCommand(string name, out ICommand command);
        IEnumerable<ICommand> ListCommands();
    }
}
