namespace kuiper.Commands
{
    public interface IConsoleCommand
    {
        string Name { get; }
        string Description { get; }
        Task<string> ExecuteAsync(string[] args, IServiceProvider services, CancellationToken cancellationToken);
    }
}
