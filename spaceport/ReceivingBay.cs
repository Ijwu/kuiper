using kbo.littlerocks;
using spaceport.schematics;

namespace spaceport;

/// <summary>
/// Faciliates realtime handling of received packets.
/// </summary>
public class ReceivingBay : IReceiveTrade
{
    private const int PacketReceiveDelayMs = 25;

    private readonly ITalkToTheServer _freighter;
    private readonly CancellationTokenSource _cts = new();
    private Task? _receiveLoopTask;

    private readonly object _handlersLock = new();
    private readonly List<PacketsReceivedHandler> _packetsReceivedHandlers = [];

    private readonly object _handlersAsyncLock = new();
    private readonly List<PacketsReceivedHandlerAsync> _packetsReceivedHandlersAsync = [];

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
        lock (_handlersLock)
        {
            _packetsReceivedHandlers.Add(handler);
        }
        return new OnPacketsReceivedHook(this, handler);
    }

    public IDisposable OnPacketsReceived(PacketsReceivedHandlerAsync handler)
    {
        lock (_handlersAsyncLock)
        {
            _packetsReceivedHandlersAsync.Add(handler);
        }
        return new OnPacketsReceivedAsyncHook(this, handler);
    }

    internal void RemovePacketsReceivedHandler(PacketsReceivedHandler handler)
    {
        lock (_handlersLock)
        {
            _packetsReceivedHandlers.Remove(handler);
        }
    }

    internal void RemovePacketsReceivedHandler(PacketsReceivedHandlerAsync handler)
    {
        lock (_handlersAsyncLock)
        {
            _packetsReceivedHandlersAsync.Remove(handler);
        }
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
        System.Console.WriteLine("Starting receive loop");
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

        GC.SuppressFinalize(this);
    }

    public class OnPacketsReceivedHook : IDisposable
    {
        private readonly ReceivingBay _bay;
        private readonly PacketsReceivedHandler _handler;

        public OnPacketsReceivedHook(ReceivingBay tradeRoute, PacketsReceivedHandler handler)
        {
            _bay = tradeRoute;
            _handler = handler;
        }

        public void Dispose()
        {
            _bay.RemovePacketsReceivedHandler(_handler);
            GC.SuppressFinalize(this);
        }
    }

    public class OnPacketsReceivedAsyncHook : IDisposable
    {
        private readonly ReceivingBay _tradeRoute;
        private readonly PacketsReceivedHandlerAsync _handler;

        public OnPacketsReceivedAsyncHook(ReceivingBay tradeRoute, PacketsReceivedHandlerAsync handler)
        {
            _tradeRoute = tradeRoute;
            _handler = handler;
        }

        public void Dispose()
        {
            _tradeRoute.RemovePacketsReceivedHandler(_handler);
            GC.SuppressFinalize(this);
        }
    }
}