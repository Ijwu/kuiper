using kbo.littlerocks;
using spaceport;

public delegate void PacketReceivedHandler(Packet packet);

public class TradeRoute
{
    private Freighter _freighter;

    public event PacketReceivedHandler? PacketReceived;

    public TradeRoute(Freighter freighter)
    {
        _freighter = freighter;
        StartReceiveLoop();
    }

    private async void StartReceiveLoop()
    {
        while (true)
        {
            Packet packet = await _freighter.ReceivePacketAsync();
            PacketReceived?.Invoke(packet);
        }
    }
}