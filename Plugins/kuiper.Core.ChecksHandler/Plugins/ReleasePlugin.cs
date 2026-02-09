using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.ChecksHandler.Plugins
{
    public class ReleasePlugin : BasePlugin
    {
        private readonly MultiData _multiData;
        private readonly IServerAnnouncementService _announcementService;
        private readonly IReleaseService _releaseService;

        public ReleasePlugin(
            ILogger<ReleasePlugin> logger,
            IConnectionManager connectionManager,
            MultiData multiData,
            IServerAnnouncementService announcementService,
            IReleaseService releaseService)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _releaseService = releaseService ?? throw new ArgumentNullException(nameof(releaseService));
        }

        protected override void RegisterHandlers()
        {
            Handle<StatusUpdate>(HandleStatusUpdateAsync);
        }

        private async Task HandleStatusUpdateAsync(StatusUpdate statusPacket, string connectionId)
        {
            if (statusPacket.Status != ClientStatus.Goal)
                return;

            var (success, slotId) = await TryGetSlotForConnectionAsync(connectionId);
            if (!success)
                return;

            await _announcementService.AnnounceGoalReachedAsync(slotId, GetPlayerName(slotId));

            var releaseMode = ParsePermission(_multiData.ServerOptions.GetValueOrDefault("release_mode")?.ToString());

            if (releaseMode == CommandPermission.Auto || releaseMode == CommandPermission.AutoEnabled)
            {
                await _releaseService.ReleaseRemainingItemsAsync(slotId);
            }
        }

        private string GetPlayerName(long slotId)
        {
            if (_multiData.SlotInfo.TryGetValue((int)slotId, out var info))
                return info.Name;
            return $"Player {slotId}";
        }

        private static CommandPermission ParsePermission(string? value) => value switch
        {
            "enabled" => CommandPermission.Enabled,
            "disabled" => CommandPermission.Disabled,
            "auto" => CommandPermission.Auto,
            "auto-enabled" => CommandPermission.AutoEnabled,
            "goal" => CommandPermission.Goal,
            _ => CommandPermission.Disabled
        };
    }
}
