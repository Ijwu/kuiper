using kbo.littlerocks;
using spaceport;

public delegate void PacketsReceivedHandler(Packet[] packets);

public class TradeRoute
{
    private Freighter _freighter;

    public event PacketsReceivedHandler? PacketReceived;

    public TradeRoute(Freighter freighter)
    {
        _freighter = freighter;
        StartReceiveLoop();
    }

    private async void StartReceiveLoop()
    {
        while (true)
        {
            Packet[] packets = await _freighter.ReceivePacketsAsync();
            PacketReceived?.Invoke(packets);
        }
    }
}