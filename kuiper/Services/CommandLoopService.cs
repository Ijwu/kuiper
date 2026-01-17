using kuiper.Commands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace kuiper.Services
{
    public class CommandLoopService : BackgroundService
    {
        private readonly CommandRegistry _registry;
        private readonly IServiceProvider _services;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger<CommandLoopService> _logger;

        public CommandLoopService(
            CommandRegistry registry,
            IEnumerable<IConsoleCommand> commands,
            IServiceProvider services,
            IHostApplicationLifetime lifetime,
            ILogger<CommandLoopService> logger)
        {
            _registry = registry;
            _services = services;
            _lifetime = lifetime;
            _logger = logger;

            foreach (var cmd in commands)
            {
                _registry.Register(cmd);
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
                    Task<string?> readLineTask = Task.Run(() => Console.ReadLine(), stoppingToken);
                    
                    // We need to wait for input or cancellation
                    // Console.ReadLine is blocking and cannot be purely cancelled without potentially leaving a dangling read.
                    // However, we rely on the process confusing on ApplicationStopping but pure stoppingToken might trigger first?
                    // In BackgroundService, stoppingToken triggers when host shuts down.
                    
                    // A simple await here is what the previous code did basically.
                    // The previous code used lifetime.ApplicationStopping as token for Task.Run
                    
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

                    if (_registry.TryGet(commandName, out var command))
                    {
                        // Create a scope for the command execution if needed, although we are passing _services (root).
                        // The original code used a scope.
                        try 
                        {
                            using var scope = _services.CreateScope();
                            var result = await command.ExecuteAsync(args, scope.ServiceProvider, stoppingToken);
                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                Console.WriteLine(result);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing command '{CommandName}'", commandName);
                            Console.WriteLine($"Error executing command: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unknown command. Type 'help' for list.");
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
