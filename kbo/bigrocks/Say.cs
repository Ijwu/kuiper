namespace kbo.bigrocks;

public record Say : Packet
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    public Say(string text) : base("Say")
    {
        Text = text;
    }
}
