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
await freighter.ConnectAsync(new Uri("wss://archipelago.gg:59323"));

TradeRoute tradeRoute = new TradeRoute(freighter);
tradeRoute.PacketsReceived += (packets) =>
{
    foreach (var packet in packets)
    {
        Console.WriteLine(packet);
    }
};

await freighter.SendPacketsAsync([new Connect("", "Blasphemous", "p1", Guid.NewGuid(), new Version(40, 0, 0), 0, new string[] { "tag" }, false)]);

Thread.Sleep(500);
await freighter.DisconnectAsync();