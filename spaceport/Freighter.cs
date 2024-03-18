using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using kbo.littlerocks;
using spaceport.schematics;

namespace spaceport;

/// <summary>
/// A class which facilitates client and server interaction.
/// </summary>
public class Freighter : ITalkToTheServer
{
    private ClientWebSocket _client = new ClientWebSocket();

    public bool IsConnected => _client.State == WebSocketState.Open;

    public async Task ConnectAsync(Uri serverUri, CancellationToken cancellationToken = default)
    {
        await _client.ConnectAsync(serverUri, cancellationToken);
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", cancellationToken);
    }

    public async Task SendPacketsAsync(Packet[] packets, CancellationToken cancellationToken = default)
    {
        string jsonString = JsonSerializer.Serialize(packets);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
        Console.WriteLine(jsonString);
        var buffer = new ArraySegment<byte>(bytes, 0, bytes.Length);
        await _client.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }

    public async Task<Packet[]> ReceivePacketsAsync(CancellationToken cancellationToken = default)
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);
        WebSocketReceiveResult result;
        using (var ms = new MemoryStream())
        {
            do
            {
                result = await _client.ReceiveAsync(buffer, cancellationToken);
                ms.Write(buffer.Array!, buffer.Offset, result.Count);
            }
            while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(ms, Encoding.UTF8))
            {
                string jsonString = reader.ReadToEnd();
                Packet[]? packets = JsonSerializer.Deserialize<Packet[]>(jsonString);
                return packets ?? [];
            }
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
