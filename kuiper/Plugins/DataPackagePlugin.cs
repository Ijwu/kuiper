using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class DataPackagePlugin : BasePlugin
    {
        private readonly MultiData _multiData;

        public DataPackagePlugin(ILogger<DataPackagePlugin> logger, MultiData multiData, WebSocketConnectionManager connectionManager)
            : base(logger, connectionManager)
        {
            _multiData = multiData ?? throw new ArgumentNullException(nameof(multiData));
        }

        protected override void RegisterHandlers()
        {
            Handle<GetDataPackage>(HandleGetDataPackageAsync);
        }

        private async Task HandleGetDataPackageAsync(GetDataPackage packet, string connectionId)
        {
            Logger.LogDebug("Handling GetDataPackagePacket for connection {ConnectionId}", connectionId);

            var dataPackagePacket = _multiData.ToDataPackage();
            await SendToConnectionAsync(connectionId, dataPackagePacket);

            Logger.LogInformation("Sent DataPackagePacket to connection {ConnectionId}", connectionId);
        }
    }
}
