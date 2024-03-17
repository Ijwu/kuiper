namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextJsonMessagePart), "text")]
[JsonDerivedType(typeof(PlayerIdJsonMessagePart), "player_id")]
[JsonDerivedType(typeof(PlayerNameJsonMessagePart), "player_name")]
[JsonDerivedType(typeof(ItemIdJsonMessagePart), "item_id")]
[JsonDerivedType(typeof(ItemNameJsonMessagePart), "item_name")]
[JsonDerivedType(typeof(LocationIdJsonMessagePart), "location_id")]
[JsonDerivedType(typeof(LocationNameJsonMessagePart), "location_name")]
[JsonDerivedType(typeof(EntranceNameJsonMessagePart), "entrance_name")]
[JsonDerivedType(typeof(ColorJsonMessagePart), "color")]
public abstract record JsonMessagePart();

public record TextJsonMessagePart : JsonMessagePart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    public TextJsonMessagePart(string? text)
    {
        Text = text;
    }
}

public record PlayerIdJsonMessagePart : TextJsonMessagePart
{
    public PlayerIdJsonMessagePart(string? text) : base(text)
    {
    }
 
}

public record PlayerNameJsonMessagePart : TextJsonMessagePart
{
    public PlayerNameJsonMessagePart(string? text) : base(text)
    {
    }
}

public record ItemIdJsonMessagePart : TextJsonMessagePart
{
    [JsonPropertyName("flags")]
    public NetworkItemFlags? Flags { get; set; }

    [JsonPropertyName("player")]
    public long? Player { get; set; }

    public ItemIdJsonMessagePart(string? text, NetworkItemFlags? flags, long? player) : base(text)
    {
        Flags = flags;
        Player = player;
    }
}

public record ItemNameJsonMessagePart: ItemIdJsonMessagePart
{
    public ItemNameJsonMessagePart(string? text, NetworkItemFlags? flags, long? player) : base(text, flags, player)
    {
    }
}

public record LocationIdJsonMessagePart : TextJsonMessagePart
{

    [JsonPropertyName("player")]
    public long? Player { get; set; }

    public LocationIdJsonMessagePart(string? text, long? player) : base(text)
    {
        Player = player;
    }
}

public record LocationNameJsonMessagePart : LocationIdJsonMessagePart
{
    public LocationNameJsonMessagePart(string? text, long? player) : base(text, player)
    {
    }
}

public record EntranceNameJsonMessagePart: TextJsonMessagePart
{
    public EntranceNameJsonMessagePart(string? text) : base(text)
    {
    }

}

public record ColorJsonMessagePart : TextJsonMessagePart
{
    [JsonPropertyName("color")]
    public string? Color { get; set; }

    public ColorJsonMessagePart(string? text, string? color) : base(text)
    {
        Color = color;
    }
}