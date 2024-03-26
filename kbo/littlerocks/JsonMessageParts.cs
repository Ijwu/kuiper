namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Text), "text")]
[JsonDerivedType(typeof(PlayerId), "player_id")]
[JsonDerivedType(typeof(PlayerName), "player_name")]
[JsonDerivedType(typeof(ItemId), "item_id")]
[JsonDerivedType(typeof(ItemName), "item_name")]
[JsonDerivedType(typeof(LocationId), "location_id")]
[JsonDerivedType(typeof(LocationName), "location_name")]
[JsonDerivedType(typeof(EntranceName), "entrance_name")]
[JsonDerivedType(typeof(Color), "color")]
public abstract record JsonMessagePart();

public record Text : JsonMessagePart
{
    [JsonPropertyName("text")]
    public string? Value { get; set; }

    public Text(string? value)
    {
        Value = value;
    }
}

public record PlayerId : Text
{
    public PlayerId(string? value) : base(value)
    {
    }
 
}

public record PlayerName : Text
{
    public PlayerName(string? value) : base(value)
    {
    }
}

public record ItemId : Text
{
    [JsonPropertyName("flags")]
    public NetworkItemFlags? Flags { get; set; }

    [JsonPropertyName("player")]
    public long? Player { get; set; }

    public ItemId(string? value, NetworkItemFlags? flags, long? player) : base(value)
    {
        Flags = flags;
        Player = player;
    }
}

public record ItemName: ItemId
{
    public ItemName(string? value, NetworkItemFlags? flags, long? player) : base(value, flags, player)
    {
    }
}

public record LocationId : Text
{

    [JsonPropertyName("player")]
    public long? Player { get; set; }

    public LocationId(string? value, long? player) : base(value)
    {
        Player = player;
    }
}

public record LocationName : LocationId
{
    public LocationName(string? value, long? player) : base(value, player)
    {
    }
}

public record EntranceName: Text
{
    public EntranceName(string? value) : base(value)
    {
    }

}

public record Color : Text
{
    [JsonPropertyName("color")]
    public string? ColorName { get; set; }

    public Color(string? value, string? color) : base(value)
    {
        ColorName = color;
    }
}