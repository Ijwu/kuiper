using kbo.littlerocks;

namespace kuiper.Plugins
{
    public interface IPlugin
    {
        Task ReceivePacket(Packet packet, string connectionId);
    }
}
