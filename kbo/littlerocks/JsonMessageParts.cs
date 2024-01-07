namespace kbo.littlerocks;

public record class BaseJsonMessagePart
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

public record class FlagsJsonMessagePart : TextJsonMessagePart
{
    [JsonPropertyName("flags")]
    public long Flags { get; set; }

    public FlagsJsonMessagePart(string? messagePartType, string? text, long flags) : base(messagePartType, text)
    {
        Flags = flags;
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