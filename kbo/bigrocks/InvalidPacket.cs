namespace kbo.bigrocks;

public record InvalidPacket : Packet
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("original_cmd")]
    public string? OriginalCommand { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    public InvalidPacket(string type, string text, string? originalCmd = null) : base("InvalidPacket")
    {
        Type = type;
        Text = text;
        OriginalCommand = originalCmd;
    }
}
