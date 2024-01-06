using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using kbo.littlerocks;

namespace spaceport;

/// <summary>
/// A class which facilitates client and server interaction.
/// </summary>
public class Freighter
{
    private ClientWebSocket _client = new ClientWebSocket();

    public async Task ConnectAsync(string ipAddress)
    {
        Uri serverUri = new Uri($"wss://{ipAddress}");
        await _client.ConnectAsync(serverUri, CancellationToken.None);
    }

    public async Task SendPacketAsync(Packet packet)
    {
        string jsonString = JsonSerializer.Serialize(packet);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
        var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
        await _client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }
}
