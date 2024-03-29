using kbo.littlerocks;

namespace spaceport.schematics;

public interface ITalkToTheServer : IDisposable
{
    bool IsConnected { get; }
    Task ConnectAsync(Uri serverUri, CancellationToken cancellationToken = default);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
    Task SendPacketsAsync(Packet[] packets, CancellationToken cancellationToken = default);
    Task<Packet[]> ReceivePacketsAsync(CancellationToken cancellationToken = default);
}
