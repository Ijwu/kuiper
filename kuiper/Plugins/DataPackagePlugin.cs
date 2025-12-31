using kbo.bigrocks;
using kbo.littlerocks;

using kuiper.Pickle;
using kuiper.Services;

namespace kuiper.Plugins
{
    public class DataPackagePlugin : IPlugin
    {
        private readonly ILogger<DataPackagePlugin> _logger;
        private readonly MultiData _multiData;
        private readonly WebSocketConnectionManager _connectionManager;

        public DataPackagePlugin(ILogger<DataPackagePlugin> logger, MultiData multiData, WebSocketConnectionManager connectionManager)
        {
            _logger = logger;
            _multiData = multiData;
            _connectionManager = connectionManager;
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            if (packet is not GetDataPackage getDataPackagePacket)
                return;

            _logger.LogDebug("Handling GetDataPackagePacket for connection {ConnectionId}", connectionId);

            var dataPackagePacket = _multiData.ToDataPackage();

            try
            {
                await _connectionManager.SendJsonToConnectionAsync(connectionId, new[] { dataPackagePacket });
                _logger.LogInformation("Sent DataPackagePacket to connection {ConnectionId}", connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send DataPackagePacket to connection {ConnectionId}", connectionId);
            }
        }
    }
}
