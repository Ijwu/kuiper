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

    public async Task ConnectAsync(Uri serverUri)
    {
        await _client.ConnectAsync(serverUri, CancellationToken.None);
    }

    public async Task SendPacketsAsync(Packet[] packets)
    {
        string jsonString = JsonSerializer.Serialize(packets);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
        var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
        await _client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task<Packet[]> ReceivePacketsAsync()
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);
        WebSocketReceiveResult result;
        using (var ms = new MemoryStream())
        {
            do
            {
                result = await _client.ReceiveAsync(buffer, CancellationToken.None);
                ms.Write(buffer.Array!, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(ms, Encoding.UTF8))
            {
                string jsonString = reader.ReadToEnd();
                Packet[] packets = JsonSerializer.Deserialize<Packet[]>(jsonString);
                return packets;
            }
        }
    }
}
