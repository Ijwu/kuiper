using System.Text.Json;
using kbo.bigrocks;
using kbo.littlerocks;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class UpdateHintPlugin : BasePlugin
    {
        private const string NotifyPrefix = "#setnotify:";

        private readonly IHintService _hintService;
        private readonly IStorageService _storage;

        public UpdateHintPlugin(ILogger<UpdateHintPlugin> logger, IHintService hintService, IStorageService storage, WebSocketConnectionManager connectionManager)
            : base(logger, connectionManager)
        {
            _hintService = hintService ?? throw new ArgumentNullException(nameof(hintService));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void RegisterHandlers()
        {
            Handle<UpdateHint>(HandleUpdateHintAsync);
        }

        private async Task HandleUpdateHintAsync(UpdateHint update, string connectionId)
        {
            var slotId = update.Player;
            var hints = await _hintService.GetHintsAsync(slotId);
            var existing = hints.FirstOrDefault(h => h.Location == update.Location);
            if (existing is null)
            {
                Logger.LogDebug("UpdateHint for slot {Slot} location {Location} has no matching hint", slotId, update.Location);
                return;
            }

            var status = update.Status ?? HintStatus.Unspecified;
            await _hintService.UpdateHintAsync(slotId, existing, status);

            await NotifySubscribersAsync(slotId);
        }

        private async Task NotifySubscribersAsync(long slotId)
        {
            var readKey = $"_read_hints_0_{slotId}";
            var keys = await _storage.ListKeysAsync();
            foreach (var key in keys)
            {
                if (!key.StartsWith(NotifyPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var connectionId = key.Substring(NotifyPrefix.Length);
                var subscriptions = await _storage.LoadAsync<string[]>(key) ?? Array.Empty<string>();
                if (!subscriptions.Any(k => string.Equals(k, readKey, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var hints = await _hintService.GetHintsAsync(slotId);
                var node = JsonSerializer.SerializeToNode(hints);
                if (node == null)
                    continue;

                var reply = new SetReply(readKey, node, node, slotId);
                await SendToConnectionAsync(connectionId, reply);
            }
        }
    }
}
