namespace kbo.bigrocks;

public record ReceivedItems : Packet
{
    [JsonPropertyName("index")]
    public long Index { get; set; }

    [JsonPropertyName("items")]
    public NetworkItem[] Items { get; set; }

    public ReceivedItems(long index, NetworkItem[] items)
    {
        Index = index;
        Items = items;
    }
}
