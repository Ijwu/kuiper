using kbo.littlerocks;

namespace spaceport.schematics;

public interface ITalkToTheServer : IDisposable
{
    bool IsConnected { get; }
    Task ConnectAsync(Uri serverUri, CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);
    Task SendPacketsAsync(Packet[] packets, CancellationToken cancellationToken);
    Task<Packet[]> ReceivePacketsAsync(CancellationToken cancellationToken);
}
