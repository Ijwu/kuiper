using System.Net.WebSockets;

using kuiper.Pickle;

namespace kuiper
{
    public class PlayerData
    {
        public required MultiDataNetworkSlot Slot { get; set; }
        public required WebSocket Socket { get; set; }
    }
}
