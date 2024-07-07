namespace kbo.littlerocks;

/*
 example PrintJSON for reference
"[
    {
        "cmd":"PrintJSON","data":[
        {
            "text":"a (Team #1) playing Blasphemous has joined. Client(5.0.0), []."
        }],
        "type":"Join",
        "team":0,
        "slot":1,
        "tags":[]
    }
]"

*/
public class JsonMessagePart
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type", IgnoreUnrecognizedTypeDiscriminators = true, UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(PlayerId), "player_id")]
    [JsonDerivedType(typeof(PlayerName), "player_name")]
    [JsonDerivedType(typeof(ItemId), "item_id")]
    [JsonDerivedType(typeof(ItemName), "item_name")]
    [JsonDerivedType(typeof(LocationId), "location_id")]
    [JsonDerivedType(typeof(LocationName), "location_name")]
    [JsonDerivedType(typeof(EntranceName), "entrance_name")]
    [JsonDerivedType(typeof(Color), "color")]
    public record Text
    {
        [JsonPropertyName("text")]
        public string? Value { get; set; }

        public Text(string? value)
        {
            Value = value;
        }

        [JsonConstructor]
        internal Text() { }
    }

    public record PlayerId : Text
    {
        public PlayerId(string? value) : base(value)
        {
        }

        [JsonConstructor]
        internal PlayerId() { }
    }

    public record PlayerName : Text
    {
        public PlayerName(string? value) : base(value)
        {
        }

        [JsonConstructor]
        internal PlayerName() { }
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

        [JsonConstructor]
        internal ItemId() { }
    }

    public record ItemName : ItemId
    {
        public ItemName(string? value, NetworkItemFlags? flags, long? player) : base(value, flags, player)
        {
        }

        [JsonConstructor]
        internal ItemName() { }
    }

    public record LocationId : Text
    {

        [JsonPropertyName("player")]
        public long? Player { get; set; }

        public LocationId(string? value, long? player) : base(value)
        {
            Player = player;
        }

        [JsonConstructor]
        internal LocationId() { }
    }

    public record LocationName : LocationId
    {
        public LocationName(string? value, long? player) : base(value, player)
        {
        }

        [JsonConstructor]
        internal LocationName() { }
    }

    public record EntranceName : Text
    {
        public EntranceName(string? value) : base(value)
        {
        }

        [JsonConstructor]
        internal EntranceName() { }
    }

    public record Color : Text
    {
        [JsonPropertyName("color")]
        public string? ColorName { get; set; }

        public Color(string? value, string? color) : base(value)
        {
            ColorName = color;
        }

        [JsonConstructor]
        internal Color() { }
    }
}