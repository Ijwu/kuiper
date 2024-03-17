namespace kbo.littlerocks;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(PlayerJsonMessagePart), "location_id")]
[JsonDerivedType(typeof(PlayerAndFlagsJsonMessagePart), "item_id")]
[JsonDerivedType(typeof(ColorJsonMessagePart), "color")]
public abstract record class TextJsonMessagePart
{
    [JsonPropertyName("type")]
    public string? MessagePartType { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    public TextJsonMessagePart(string? messagePartType, string? text)
    {
        MessagePartType = messagePartType;
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