namespace kbo;

public record Get : Packet
{
    [JsonPropertyName("keys")]
    public string[] Keys { get; set; }

    public Get(string[] keys)
    {
        Keys = keys;
    }
}
