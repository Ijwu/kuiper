using kbo.littlerocks;

namespace spaceport.schematics;

public delegate void PacketsReceivedHandler(Packet[] packets);
public delegate Task PacketsReceivedHandlerAsync(Packet[] packets);
public interface IReceiveTrade : IDisposable
{
    IDisposable OnPacketsReceived(PacketsReceivedHandler handler);
    IDisposable OnPacketsReceived(PacketsReceivedHandlerAsync handler);
}
