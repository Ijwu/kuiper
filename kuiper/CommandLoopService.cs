using kuiper.Commands;
using kuiper.Commands.Abstract;

namespace kuiper.Services
{
    public class CommandLoopService : BackgroundService
    {
        private readonly ICommandRegistry _registry;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<CommandLoopService> _logger;

        public CommandLoopService(
            ICommandRegistry registry,
            IEnumerable<ICommand> commands,
            IHostApplicationLifetime lifetime,
            ILogger<CommandLoopService> logger)
        {
            _registry = registry;
            _lifetime = lifetime;
            _logger = logger;

            foreach (var cmd in commands)
            {
                registry.RegisterCommand(cmd);
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Yield to ensure the startup path continues while this runs
            await Task.Yield();

            _logger.LogInformation("Command loop started. Type 'help' for commands.");

            try
            {
                while (!stoppingToken.IsCancellationRequested && !_lifetime.ApplicationStopping.IsCancellationRequested)
                {
                    Task<string?> readLineTask = Task.Run(Console.ReadLine, stoppingToken);

                    string? line = null;
                    try 
                    {
                        line = await readLineTask; // This effectively blocks until enter is pressed.
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                    if (line is null)
                    {
                        await Task.Delay(100, stoppingToken);
                        continue;
                    }

                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                        continue;

                    var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var commandName = parts[0];
                    var args = parts.Skip(1).ToArray();

                    if (_registry.TryGetCommand(commandName, out var command))
                    {
                        try 
                        {
                            var result = await command.ExecuteAsync(args, -1, stoppingToken);
                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                _logger.LogInformation("{Result}", result);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing command '{CommandName}'", commandName);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Unknown command '{CommandName}'. Type 'help' for list.", commandName);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Command loop failed");
            }
        }
    }
}
