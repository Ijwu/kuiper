using kbo.littlerocks;
using spaceport;

public delegate void PacketsReceivedHandler(Packet[] packets);

public class TradeRoute : IDisposable
{
    private const int PacketReceiveDelayMs = 25;

    private readonly Freighter _freighter;
    private readonly Task _receiveLoopTask;
    private readonly CancellationTokenSource _cts = new();

    public event PacketsReceivedHandler? PacketsReceived;

    public TradeRoute(Freighter freighter)
    {
        _freighter = freighter;
        _receiveLoopTask = Task.Run(() => StartReceiveLoop());
    }

    private async Task StartReceiveLoop()
    {
        while (true)
        {
            if (_cts.Token.IsCancellationRequested)
                _cts.Token.ThrowIfCancellationRequested();

            Packet[] packets = await _freighter.ReceivePacketsAsync(_cts.Token);
            PacketsReceived?.Invoke(packets);

            await Task.Delay(PacketReceiveDelayMs);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _receiveLoopTask.Wait();
        _cts.Dispose();
    }
}