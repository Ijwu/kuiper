using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Commands.Abstract;
using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.Chats.Plugins
{
    /// <summary>
    /// Handles Say packets that start with '!' by executing the remainder as a console command
    /// and returning the output to the sender as a PrintJson packet.
    /// </summary>
    public class SayCommandPlugin : BasePlugin
    {
        private readonly ICommandRegistry _registry;
        private readonly IKuiperConfigService _config;

        public SayCommandPlugin(ILogger<SayCommandPlugin> logger,
                                ICommandRegistry registry,
                                IConnectionManager connectionManager,
                                INotifyingStorageService storage,
                                IKuiperConfigService config,
                                IHintService hintService,
                                IHintPointsService hintPointsService,
                                MultiData multiData,
                                IServerAnnouncementService announcementService)
            : base(logger, connectionManager)
        {
            _registry = registry;
            _config = config;
        }

        protected override void RegisterHandlers()
        {
            Handle<Say>(HandleSayAsync);
        }

        private async Task HandleSayAsync(Say say, string connectionId)
        {
            if (string.IsNullOrWhiteSpace(say.Text))
                return;

            string prefix = "!";
            try
            {
                var configuredPrefix = _config.GetServerConfig<string>("Server:IngameCommandPrefix");
                if (!string.IsNullOrEmpty(configuredPrefix))
                {
                    prefix = configuredPrefix;
                }
            }
            catch
            {
                // Fallback to default
            }

            string text = say.Text.Trim();
            if (!text.StartsWith(prefix))
                return;

            long? slotId = await ConnectionManager.GetSlotForConnectionAsync(connectionId);
            if (!slotId.HasValue)
            {
                await SendOutputAsync(connectionId, "Command rejected: connection not mapped to a slot.");
                return;
            }

            string commandLine = text[prefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(commandLine))
            {
                await SendOutputAsync(connectionId, "Command rejected: no command provided.");
                return;
            }

            string[] parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? commandName = parts.FirstOrDefault();
            string[] args = parts.Skip(1).ToArray();

            if (string.IsNullOrEmpty(commandName))
            {
                await SendOutputAsync(connectionId, "Command rejected: no command name provided.");
                return;
            }

            if (!_registry.TryGetCommand(commandName, out var command))
            {
                await SendOutputAsync(connectionId, $"Unknown command '{commandName}'.");
                return;
            }

            try
            {
                string result = await command.ExecuteAsync(args, slotId!.Value, CancellationToken.None);
                string output = string.IsNullOrWhiteSpace(result) ? "(no output)" : result;
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
            var print = new PrintJson([new JsonMessagePart.Text(output)]);
            await SendToConnectionAsync(connectionId, print);
        }
    }
}
