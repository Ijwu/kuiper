using kbo.bigrocks;

using kuiper.Core.Pickle;
using kuiper.Core.Services.Abstract;
using kuiper.Plugins;

using Microsoft.Extensions.Logging;

namespace kuiper.Core.DataPackages.Plugins
{
    public class DataPackagePlugin : BasePlugin
    {
        private readonly MultiData _multiData;

        public DataPackagePlugin(ILogger<DataPackagePlugin> logger, MultiData multiData, IConnectionManager connectionManager)
            : base(logger, connectionManager)
        {
            _multiData = multiData;
        }

        protected override void RegisterHandlers()
        {
            Handle<GetDataPackage>(HandleGetDataPackageAsync);
        }

        private async Task HandleGetDataPackageAsync(GetDataPackage packet, string connectionId)
        {
            var dataPackagePacket = _multiData.ToDataPackage();
            await SendToConnectionAsync(connectionId, dataPackagePacket);

            Logger.LogDebug("Sent DataPackagePacket to connection {ConnectionId}", connectionId);
        }
    }
}
