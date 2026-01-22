using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;
using kuiper.Services.Abstract;

namespace kuiper.Plugins
{
    public class ReleasePlugin : BasePlugin
    {
        private class ReleasePluginConfig
        {
            public bool AutoReleaseOnGoal { get; set; } = true;
        }

        private readonly MultiData _multiData;
        private readonly IServerAnnouncementService _announcementService;
        private readonly IKuiperConfig _config;
        private readonly IReleaseService _releaseService;

        public ReleasePlugin(
            ILogger<ReleasePlugin> logger,
            WebSocketConnectionManager connectionManager,
            MultiData multiData,
            IServerAnnouncementService announcementService,
            IKuiperConfig config,
            IReleaseService releaseService)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
            _announcementService = announcementService ?? throw new ArgumentNullException(nameof(announcementService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
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

            var config = _config.GetPluginConfig<ReleasePluginConfig>("ReleasePlugin");
            if (config?.AutoReleaseOnGoal ?? true)
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
    }
}
