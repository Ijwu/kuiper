using kbo.littlerocks;
using spaceport;
using spaceport.schematics;

/// <summary>
/// Faciliates realtime handling of received packets.
/// </summary>
public class ReceivingBay : IReceiveTrade
{
    private const int PacketReceiveDelayMs = 25;

    private readonly ITalkToTheServer _freighter;
    private readonly CancellationTokenSource _cts = new();
    private Task? _receiveLoopTask;

    private List<PacketsReceivedHandler> _packetsReceivedHandlers = [];
    private List<PacketsReceivedHandlerAsync> _packetsReceivedHandlersAsync = [];

    public ReceivingBay(ITalkToTheServer cargoTransport)
    {
        _freighter = cargoTransport;
    }

    public void StartReceiving()
    {
        if (_freighter.IsConnected && _receiveLoopTask == null)
        {
            _receiveLoopTask = Task.Run(() => StartReceiveLoop());
        }
        else
        {
            throw new InvalidOperationException("TradeRoute cannot start receiving packets if it is not connected to a server or if it is already receiving packets.");
        }
    }

    public IDisposable OnPacketsReceived(PacketsReceivedHandler handler)
    {
        _packetsReceivedHandlers.Add(handler);
        return new OnPacketsReceivedHook(this, handler);
    }

    public IDisposable OnPacketsReceived(PacketsReceivedHandlerAsync handler)
    {
        _packetsReceivedHandlersAsync.Add(handler);
        return new OnPacketsReceivedAsyncHook(this, handler);
    }

    internal void RemovePacketsReceivedHandler(PacketsReceivedHandler handler)
    {
        _packetsReceivedHandlers.Remove(handler);
    }

    internal void RemovePacketsReceivedHandler(PacketsReceivedHandlerAsync handler)
    {
        _packetsReceivedHandlersAsync.Remove(handler);
    }

    private void PacketsReceived(Packet[] packets)
    {
        foreach (var handler in _packetsReceivedHandlers)
        {
            handler(packets);
        }
    }

    private async Task PacketsReceivedAsync(Packet[] packets)
    {
        var tasks = new List<Task>();
        foreach (var handler in _packetsReceivedHandlersAsync)
        {
            tasks.Add(handler(packets));
        }
        PacketsReceived(packets);
        await Task.WhenAll(tasks);
    }

    private async Task StartReceiveLoop()
    {
        while (true)
        {
            if (_cts.Token.IsCancellationRequested)
                _cts.Token.ThrowIfCancellationRequested();

            Packet[] packets = await _freighter.ReceivePacketsAsync(_cts.Token);
            await PacketsReceivedAsync(packets);

            await Task.Delay(PacketReceiveDelayMs);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _receiveLoopTask?.Wait();
        _freighter.Dispose();
        _cts.Dispose();
    }

    public class OnPacketsReceivedHook : IDisposable
    {
        private readonly ReceivingBay _bay;
        private PacketsReceivedHandler _handler;

        public OnPacketsReceivedHook(ReceivingBay tradeRoute, PacketsReceivedHandler handler)
        {
            _bay = tradeRoute;
            _handler = handler;
        }

        public void Dispose()
        {
            _bay.RemovePacketsReceivedHandler(_handler);
        }
    }

    public class OnPacketsReceivedAsyncHook : IDisposable
    {
        private readonly ReceivingBay _tradeRoute;
        private PacketsReceivedHandlerAsync _handler;

        public OnPacketsReceivedAsyncHook(ReceivingBay tradeRoute, PacketsReceivedHandlerAsync handler)
        {
            _tradeRoute = tradeRoute;
            _handler = handler;
        }

        public void Dispose()
        {
            _tradeRoute.RemovePacketsReceivedHandler(_handler);
        }
    }
}