using kbo.littlerocks;

using kuiper.Core.Services.Abstract;
using kuiper.Plugins.Abstract;

namespace kuiper.Plugins
{
    /// <summary>
    /// Base class for plugins that provides common functionality like slot resolution,
    /// error handling, and logging patterns. Supports handling multiple packet types.
    /// </summary>
    public abstract class BasePlugin : IKuiperPlugin
    {
        protected readonly ILogger Logger;
        protected readonly IConnectionManager ConnectionManager;

        private readonly Dictionary<Type, Func<Packet, string, Task>> _handlers = new();

        protected BasePlugin(ILogger logger, IConnectionManager connectionManager)
        {
            Logger = logger;
            ConnectionManager = connectionManager;
            RegisterHandlers();
        }

        /// <summary>
        /// Override this method to register packet handlers using <see cref="Handle{TPacket}"/>.
        /// </summary>
        protected abstract void RegisterHandlers();

        /// <summary>
        /// Registers a handler for a specific packet type.
        /// </summary>
        protected void Handle<TPacket>(Func<TPacket, string, Task> handler) where TPacket : Packet
        {
            _handlers[typeof(TPacket)] = (packet, connectionId) => handler((TPacket)packet, connectionId);
        }

        public async Task ReceivePacket(Packet packet, string connectionId)
        {
            var packetType = packet.GetType();
            if (!_handlers.TryGetValue(packetType, out var handler))
                return;

            try
            {
                await handler(packet, connectionId);
                Logger.LogDebug("Handled {PacketType} packet from connection {ConnectionId}", packetType.Name, connectionId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling {PacketType} from connection {ConnectionId}", packetType.Name, connectionId);
                throw;
            }
        }

        protected async Task<long?> GetSlotForConnectionAsync(string connectionId)
        {
            var slotId = await ConnectionManager.GetSlotForConnectionAsync(connectionId);
            if (!slotId.HasValue)
            {
                Logger.LogDebug("Connection {ConnectionId} has no mapped slot.", connectionId);
            }
            return slotId;
        }

        protected async Task<(bool Success, long SlotId)> TryGetSlotForConnectionAsync(string connectionId)
        {
            var slotId = await GetSlotForConnectionAsync(connectionId);
            return slotId.HasValue ? (true, slotId.Value) : (false, 0);
        }

        protected Task SendToConnectionAsync(string connectionId, params Packet[] packets)
        {
            return ConnectionManager.SendJsonToConnectionAsync(connectionId, packets);
        }

        protected async Task SendToSlotAsync(long slotId, params Packet[] packets)
        {
            var connectionIds = await ConnectionManager.GetConnectionIdsForSlotAsync(slotId);
            foreach (var connId in connectionIds)
            {
                await ConnectionManager.SendJsonToConnectionAsync(connId, packets);
            }
        }
    }
}
