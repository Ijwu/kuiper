using spaceport;
using kbo.bigrocks;
using kbo.littlerocks;
using System.Text.Json;
using System.Diagnostics;

// while (!Debugger.IsAttached)
// {
//     Thread.Sleep(100);
// }

Freighter freighter = new Freighter();
await freighter.ConnectAsync(new Uri("wss://archipelago.gg:59493"));

TradeRoute tradeRoute = new TradeRoute(freighter);
tradeRoute.PacketReceived += (packet) =>
{
    Console.WriteLine($"Received packet: {packet}");
};

await freighter.SendPacketsAsync([new Connect("", "Blasphemous", "p1", Guid.NewGuid(), new Version(1, 0, 0), 0, new string[] { "tag" }, false)]);