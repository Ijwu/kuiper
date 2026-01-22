using System.Net.WebSockets;

using kuiper.Pickle;

namespace kuiper
{
    public class PlayerData
    {
        public required WebSocket Socket { get; set; }
    }
}
