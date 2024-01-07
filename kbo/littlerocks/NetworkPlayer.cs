namespace kbo.littlerocks;

public record NetworkPlayer : NetworkObject
{
    [JsonPropertyName("team")]
    public long Team { get; set; }

    [JsonPropertyName("slot")]
    public long Slot { get; set; }

    [JsonPropertyName("alias")]
    public string Alias { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    public NetworkPlayer(long team, long slot, string alias, string name)
    {
        Team = team;
        Slot = slot;
        Alias = alias;
        Name = name;
    }
}
