using kbo.plantesimals;

namespace kbo.littlerocks;

[JsonConverter(typeof(PacketConverter))]
public record Packet
{
    [JsonPropertyName("cmd")]
    public string Cmd { get; set; } = string.Empty;
}
