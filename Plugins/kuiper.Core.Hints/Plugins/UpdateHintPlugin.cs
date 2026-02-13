using System.Text.Json;
using System.Text.Json.Nodes;

using kbo;
using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Constants;
using kuiper.Core.Services.Abstract;

using Microsoft.Extensions.Logging;

namespace kuiper.Plugins
{
    public class UpdateHintPlugin : BasePlugin
    {
        private readonly IHintService _hintService;
        private readonly INotifyingStorageService _storage;

        public UpdateHintPlugin(ILogger<UpdateHintPlugin> logger, IHintService hintService, INotifyingStorageService storage, IConnectionManager connectionManager)
            : base(logger, connectionManager)
        {
            _hintService = hintService;
            _storage = storage;
        }

        protected override void RegisterHandlers()
        {
            Handle<UpdateHint>(HandleUpdateHintAsync);
        }

        private async Task HandleUpdateHintAsync(UpdateHint update, string connectionId)
        {
            (bool success, long slotId) = await TryGetSlotForConnectionAsync(connectionId);

            if (!success)
            {
                return;
            }

            if (slotId != update.Player)
            {
                return;
            }

            Hint[] hints = await _hintService.GetHintsAsync(slotId);

            Hint? existing = hints.FirstOrDefault(h => h.Location == update.Location);
            if (existing is null)
            {
                Logger.LogDebug("UpdateHint for slot {Slot} location {Location} has no matching hint", slotId, update.Location);
                return;
            }

            if (existing.Found || existing.Status == HintStatus.Found || update.Status == HintStatus.Found)
            {
                return;
            }

            if (update.Status.HasValue)
            {
                existing.Status = update.Status.Value;
            }

            await _hintService.AddOrUpdateHintAsync(slotId, existing);

            await NotifySubscribersAsync(slotId, hints);
        }

        private async Task NotifySubscribersAsync(long slotId, Hint[] beforeUpdate)
        {
            var readKey = $"_read_hints_0_{slotId}";
            var keys = await _storage.ListKeysAsync();
            foreach (string key in keys)
            {
                if (!key.StartsWith(StorageKeys.SetNotifyPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string connectionId = key.Substring(StorageKeys.SetNotifyPrefix.Length);
                string[] subscriptions = await _storage.LoadAsync<string[]>(key) ?? [];
                if (!subscriptions.Any(k => k == readKey))
                {
                    continue;
                }

                Hint[] hints = await _hintService.GetHintsAsync(slotId);
                JsonNode? node = JsonSerializer.SerializeToNode(hints);
                if (node == null)
                {
                    continue;
                }

                JsonNode? originalNode = JsonSerializer.SerializeToNode(beforeUpdate);

                var reply = new SetReply(readKey, originalNode!, node, slotId);
                await SendToConnectionAsync(connectionId, reply);
            }
        }
    }
}
