namespace kbo.littlerocks;

public record NetworkItem : NetworkObject
{
    [JsonPropertyName("item")]
    public long Item { get; set; }

    [JsonPropertyName("location")]
    public long Location { get; set; }

    [JsonPropertyName("player")]
    public long Player { get; set; }

    [JsonPropertyName("flags")]
    public long Flags { get; set; }

    public NetworkItem(long item, long location, long player, long flags)
    {
        Item = item;
        Location = location;
        Player = player;
        Flags = flags;
    }
}
