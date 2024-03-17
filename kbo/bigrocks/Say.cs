namespace kbo.bigrocks;

public record class Say : Packet
{
    [JsonPropertyName("text")]
    public string Text { get; set; }

    public Say(string text)
    {
        Text = text;
    }
}
