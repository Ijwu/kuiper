namespace kbo.bigrocks;

public record Get : Packet
{
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }

    public Get(string[] keys) : base("Get")
    {
        Keys = keys;
    }
}
