namespace kuiper.Commands
{
    public interface IConsoleCommand
    {
        string Name { get; }
        string Description { get; }
        Task ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken);
    }
}
