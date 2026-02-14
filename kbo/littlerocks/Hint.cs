namespace kbo.littlerocks;

public record Hint
{
    [JsonPropertyName("receiving_player")]
    public long ReceivingPlayer { get; set; }

    [JsonPropertyName("finding_player")]
    public long FindingPlayer { get; set; }

    [JsonPropertyName("location")]
    public long Location { get; set; }

    [JsonPropertyName("item")]
    public long Item { get; set; }

    [JsonPropertyName("found")]
    public bool Found { get; set; }

    [JsonPropertyName("entrance")]
    public string Entrance { get; set; }

    [JsonPropertyName("item_flags")]
    public NetworkItemFlags ItemFlags { get; set; }

    [JsonPropertyName("status")]
    public HintStatus Status { get; set; }

    public Hint(long receivingPlayer, long findingPlayer, long location, long item, bool found, string entrance, NetworkItemFlags itemFlags, HintStatus status)
    {
        ReceivingPlayer = receivingPlayer;
        FindingPlayer = findingPlayer;
        Location = location;
        Item = item;
        Found = found;
        Entrance = entrance;
        ItemFlags = itemFlags;
        Status = status;
    }
}
