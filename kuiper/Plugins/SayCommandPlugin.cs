using System.Text;
using Microsoft.Extensions.DependencyInjection;
using kbo.bigrocks;
using kbo.littlerocks;
using kuiper.Commands;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    /// <summary>
    /// Handles Say packets that start with '!' by executing the remainder as a console command
    /// and returning the output to the sender as a PrintJson packet.
    /// </summary>
    public class SayCommandPlugin : BasePlugin
    {
        private static readonly object ConsoleLock = new();
        private const string AuthorizedSlotsKey = "#authorized_command_slots";

        private readonly CommandRegistry _registry;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IStorageService _storage;

        public SayCommandPlugin(ILogger<SayCommandPlugin> logger, CommandRegistry registry, IServiceScopeFactory scopeFactory, WebSocketConnectionManager connectionManager, IStorageService storage)
            : base(logger, connectionManager)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void RegisterHandlers()
        {
            Handle<Say>(HandleSayAsync);
        }

        private async Task HandleSayAsync(Say say, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(say.Text))
                return;

            var text = say.Text.Trim();
            if (!text.StartsWith("!"))
                return;

            var slotId = await ConnectionManager.GetSlotForConnectionAsync(connectionId);
            if (!slotId.HasValue)
            {
                await SendOutputAsync(connectionId, "Command rejected: connection not mapped to a slot.");
                return;
            }

            var authorized = await _storage.LoadAsync<long[]>(AuthorizedSlotsKey) ?? Array.Empty<long>();
            if (!authorized.Contains(slotId.Value))
            {
                await SendOutputAsync(connectionId, "Command rejected: slot is not authorized.");
                return;
            }

            var commandLine = text[1..].Trim();
            if (string.IsNullOrWhiteSpace(commandLine))
                return;

            var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts.FirstOrDefault();
            var args = parts.Skip(1).ToArray();

            if (string.IsNullOrEmpty(commandName) || !_registry.TryGet(commandName, out var command))
            {
                await SendOutputAsync(connectionId, $"Unknown command '{commandName}'.");
                return;
            }

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var services = scope.ServiceProvider;
                var result = await command.ExecuteAsync(args, services, CancellationToken.None);
                var output = string.IsNullOrWhiteSpace(result) ? "(no output)" : result;
                await SendOutputAsync(connectionId, output);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to execute command {Command}", commandName);
                await SendOutputAsync(connectionId, $"Command '{commandName}' failed: {ex.Message}");
            }
        }

        private async Task SendOutputAsync(string connectionId, string output)
        {
            var print = new PrintJson(new JsonMessagePart.Text[] { new JsonMessagePart.Text(output) });
            await SendToConnectionAsync(connectionId, print);
        }
    }
}
