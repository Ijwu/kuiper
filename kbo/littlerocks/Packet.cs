using kbo.plantesimals;

namespace kbo.littlerocks;

[JsonConverter(typeof(PacketConverter))]
public abstract record Packet
{
    [JsonPropertyName("cmd")]
    public string PacketType { get; protected set; }

    public Packet(string packetType)
    {
        PacketType = packetType;
    }
}
