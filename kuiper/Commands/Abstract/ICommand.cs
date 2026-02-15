namespace kuiper.Commands.Abstract
{
    public interface ICommand
    {
        string Name { get; }
        string Description { get; }
        Task<string> ExecuteAsync(string[] args, long executingSlot, CancellationToken cancellationToken);
    }
}
