using kbo.littlerocks;

namespace kuiper.Plugins.Abstract
{
    public interface IKuiperPlugin
    {
        Task ReceivePacket(Packet packet, string connectionId);
    }
}
