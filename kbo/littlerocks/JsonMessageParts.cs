namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(TextJsonMessagePart), "text")]
[JsonDerivedType(typeof(TextJsonMessagePart), "player_id")]
[JsonDerivedType(typeof(TextJsonMessagePart), "player_name")]
[JsonDerivedType(typeof(TextJsonMessagePart), "item_name")]
[JsonDerivedType(typeof(TextJsonMessagePart), "location_name")]
[JsonDerivedType(typeof(TextJsonMessagePart), "entrance_name")]
[JsonDerivedType(typeof(PlayerJsonMessagePart), "location_id")]
[JsonDerivedType(typeof(PlayerAndFlagsJsonMessagePart), "item_id")]
[JsonDerivedType(typeof(ColorJsonMessagePart), "color")]
public abstract record class BaseJsonMessagePart
{
    [JsonPropertyName("type")]
    public string? MessagePartType { get; set; }

    public BaseJsonMessagePart(string? messagePartType)
    {
        MessagePartType = messagePartType;
    }
}

public record class TextJsonMessagePart : BaseJsonMessagePart
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    public TextJsonMessagePart(string? messagePartType, string? text) : base(messagePartType)
    {
        Text = text;
    }
}

public record class ColorJsonMessagePart : TextJsonMessagePart
{
    [JsonPropertyName("color")]
    public string? Color { get; set; }

    public ColorJsonMessagePart(string? messagePartType, string? text, string? color) : base(messagePartType, text)
    {
        Color = color;
    }
}

public record class PlayerJsonMessagePart : TextJsonMessagePart
{
    [JsonPropertyName("player")]
    public long Player { get; set; }

    public PlayerJsonMessagePart(string? messagePartType, string? text, long player) : base(messagePartType, text)
    {
        Player = player;
    }    
}

public record class PlayerAndFlagsJsonMessagePart : PlayerJsonMessagePart
{
    [JsonPropertyName("flags")]
    public long Flags { get; set; }

    public PlayerAndFlagsJsonMessagePart(string? messagePartType, string? text, long player, long flags) : base(messagePartType, text, player)
    {
        Flags = flags;
    }  
}