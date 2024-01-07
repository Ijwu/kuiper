using spaceport;
using kbo.bigrocks;
using kbo.littlerocks;
using System.Text.Json;

var packets = new Packet[]
{
    new RoomInfo(new Version(1, 2, 3), new Version(4, 5, 6), new string[] { "tag1", "tag2" }, true, new Dictionary<string, int> { { "perm1", 1 }, { "perm2", 2 } }, 3, 4, new string[] { "game1", "game2" }, new Dictionary<string, int> { { "dpv1", 1 }, { "dpv2", 2 } }, new Dictionary<string, string> { { "dpc1", "c1" }, { "dpc2", "c2" } }, "seed", 1234567890),
    new Connect("poop", "poop", "poop", Guid.Empty, new Version(1, 2, 3), 0, new string[] { "tag1", "tag2" }, false)
};

var json = JsonSerializer.Serialize(packets);

var packs = JsonSerializer.Deserialize<Packet[]>(json);

foreach (Packet packet in packs)
{
    Console.WriteLine(packet);
}

return;
Freighter freighter = new Freighter();
await freighter.ConnectAsync(new Uri("wss://archipelago.gg:59493"));

TradeRoute tradeRoute = new TradeRoute(freighter);
tradeRoute.PacketReceived += (packet) =>
{
    Console.WriteLine($"Received packet: {packet}");
};

await freighter.SendPacketAsync(new Connect("password", "game", "name", Guid.NewGuid(), new Version(1, 0, 0), 0, new string[] { "tag" }, false));