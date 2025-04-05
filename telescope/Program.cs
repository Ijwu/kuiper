using System.Text.Json;

using kbo.bigrocks;
using kbo.littlerocks;

using spaceport;
using spaceport.schematics;

ITalkToTheServer freighter = new Freighter();

await freighter.ConnectAsync(new Uri("ws://localhost:38281"));

IReceiveTrade receivingBay = new ReceivingBay(freighter);

void PacketHandler(Packet[] packets)
{
    foreach (Packet packet in packets)
    {
        Console.WriteLine(packet.GetType().Name);
        Console.WriteLine(
            JsonSerializer.Serialize(packet, new JsonSerializerOptions { WriteIndented = true })
        );
    }
}

var handler = receivingBay.OnPacketsReceived(PacketHandler);

receivingBay.StartReceiving();

await freighter.SendPacketsAsync([new Connect(null,
                                        "Blasphemous",
                                        "1",
                                        Guid.NewGuid(),
                                        new Version(6,5,0),
                                        ItemHandlingFlags.All,
                                        ["DeathLink"],
                                        true)]);

while (true)
{
    // Wait for packets to be received
    await Task.Delay(1000);
    // Check if the connection is still open
    if (!freighter.IsConnected)
    {
        Console.WriteLine("Connection closed.");
        break;
    }
}