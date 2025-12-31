using System.Net.WebSockets;

using kuiper.Pickle;

namespace kuiper
{
    public class PlayerData
    {
        public MultiDataNetworkSlot Slot { get; set; }
        public WebSocket Socket { get; set; }
    }
}
