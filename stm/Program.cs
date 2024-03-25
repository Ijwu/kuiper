using spaceport;
using kbo.bigrocks;
using kbo.littlerocks;

Freighter freighter = new();
await freighter.ConnectAsync(new Uri("wss://archipelago.gg:46289"));

var receivingBay = new ReceivingBay(freighter);
receivingBay.StartReceiving();

var hook = receivingBay.OnPacketsReceived(packets =>
{
    foreach (var packet in packets)
    {
        Console.WriteLine($"Received packet: {packet}");
    }
});

Packet[] packets = [new Connect(string.Empty, "Blasphemous", "a", Guid.NewGuid(), new Version(5,0,0), ItemHandlingFlags.All, [], false)];
await freighter.SendPacketsAsync(packets);

await freighter.DisconnectAsync();