namespace kbo.bigrocks;

public record InvalidPacket : Packet
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("original_cmd")]
    public string? OriginalCmd { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    public InvalidPacket(string type, string text, string? originalCmd = null)
    {
        Type = type;
        Text = text;
        OriginalCmd = originalCmd;
    }
}
