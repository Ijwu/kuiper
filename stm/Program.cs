using spaceport;
using kbo.bigrocks;
using kbo.littlerocks;
using System.Text.Json;
using System.Diagnostics;
using kbo.plantesimals;

// while (!Debugger.IsAttached)
// {
//     Thread.Sleep(100);
// }

// Freighter freighter = new Freighter();
// await freighter.ConnectAsync(new Uri("wss://archipelago.gg:59323"));

// TradeRoute tradeRoute = new TradeRoute(freighter);
// tradeRoute.PacketsReceived += (packets) =>
// {
//     foreach (var packet in packets)
//     {
//         Console.WriteLine(packet);
//     }
// };

// await freighter.SendPacketsAsync([new Connect("", "Blasphemous", "p1", Guid.NewGuid(), new Version(40, 0, 0), 0, new string[] { "tag" }, false)]);

// Thread.Sleep(500);
// await freighter.DisconnectAsync();

var messageParts = new JsonMessagePart[]
{
    new TextJsonMessagePart("Hello, world!"),
    new PlayerIdJsonMessagePart("player_id"),
    new PlayerNameJsonMessagePart("player_name"),
    new ItemIdJsonMessagePart("item_id", NetworkItemFlags.None, 0),
    new ItemNameJsonMessagePart("item_name", NetworkItemFlags.None, 0),
    new LocationIdJsonMessagePart("location_id", 0),
    new LocationNameJsonMessagePart("location_name", 0),
    new EntranceNameJsonMessagePart("entrance_name"),
    new ColorJsonMessagePart("Hello, world!", "red")
};

var thing = new PrintJson(messageParts);
Console.WriteLine(JsonSerializer.Serialize<Packet[]>(new []{thing}, new JsonSerializerOptions { WriteIndented = true }));