namespace kbo.littlerocks;

public abstract record Packet
{
    [JsonPropertyName("cmd")]
    public string PacketType { get; protected set; }

    public Packet(string packetType)
    {
        PacketType = packetType;
    }
}
