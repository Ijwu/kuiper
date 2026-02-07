using kbo.littlerocks;

namespace kuiper.Plugins
{
    public interface IKuiperPlugin
    {
        Task ReceivePacket(Packet packet, string connectionId);
    }
}
